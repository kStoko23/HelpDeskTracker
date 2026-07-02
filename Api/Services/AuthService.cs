using Api.Common;
using Api.Data;
using Api.Entities;
using Api.Features.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AuthService
{
    private readonly HelpDeskDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _config;
    private const int ExpirationDays = 7;

    public AuthService(HelpDeskDbContext dbContext, JwtService jwtService, IConfiguration config)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _config = config;
    }

    public async Task<ServiceResult<LoginResult>> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLower());
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ServiceResult<LoginResult>.Unauthorized("Invalid credentials");
        
        if (user.MustChangePassword && user.TempPasswordExpiresAt.HasValue && user.TempPasswordExpiresAt.Value <= DateTime.UtcNow)
            return ServiceResult<LoginResult>.Forbidden("Temporary password has expired");

        if (!user.IsActive) return ServiceResult<LoginResult>.Forbidden("User account is inactive");

        var accessToken = _jwtService.GenerateJwtToken(user);
        var refreshToken = _jwtService.GetRefreshToken();
        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);

        _dbContext.RefreshTokens.RemoveRange(_dbContext.RefreshTokens.Where(x =>
            x.UserId == user.Id && (x.Revoked || x.ExpiresAt <= DateTime.UtcNow)));

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(ExpirationDays),
            CreatedAt = DateTime.UtcNow
        });

        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpirationMinutes"]!));

        await _dbContext.SaveChangesAsync();

        return ServiceResult<LoginResult>.Success(new LoginResult
        {
            AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = expiresAt, MustChangePassword = user.MustChangePassword,
        });
    }

    public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
    {
        if (await _dbContext.Users.AnyAsync(x => x.Email == request.Email.Trim().ToLower()))
            return ServiceResult.Conflict("Email already taken");

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = UserRole.Client,
            MustChangePassword = false,
            TempPasswordExpiresAt = null
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<LoginResult>> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken)) return ServiceResult<LoginResult>.Unauthorized("Refresh token missing");

        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
        var stored = await _dbContext.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash);

        if (stored == null) return ServiceResult<LoginResult>.Unauthorized("Invalid refresh token");
        if (stored.Revoked)
        {
            //revoke all tokens if already revoked used
            await _dbContext.RefreshTokens.Where(x => x.UserId == stored.User.Id && !x.Revoked)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Revoked, true));
            
            return ServiceResult<LoginResult>.Unauthorized("Refresh token has been revoked");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            return ServiceResult<LoginResult>.Unauthorized("Refresh token expired");
        if (!stored.User.IsActive) return ServiceResult<LoginResult>.Unauthorized("User account is inactive");

        stored.Revoked = true;

        var newAccesToken = _jwtService.GenerateJwtToken(stored.User);
        var newRefreshToken = _jwtService.GetRefreshToken();
        var newRefreshTokenHash = _jwtService.HashRefreshToken(newRefreshToken);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            UserId = stored.User.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(ExpirationDays),
            CreatedAt = DateTime.UtcNow
        });

        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpirationMinutes"]!));

        await _dbContext.SaveChangesAsync();

        return ServiceResult<LoginResult>.Success(new LoginResult
        {
            AccessToken = newAccesToken, RefreshToken = newRefreshToken, ExpiresAt = expiresAt, MustChangePassword = stored.User.MustChangePassword
        });
    }

    public async Task<ServiceResult> LogoutAsync(string? refreshToken)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
            var stored = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash);

            if (stored != null && !stored.Revoked)
            {
                stored.Revoked = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ChangePasswordAsync(long userId, ChangePasswordRequest request)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return ServiceResult.Unauthorized("Unauthorized");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult.Unauthorized("Current password is incorrect");

        if (request.NewPassword != request.ConfirmPassword) return ServiceResult.BadRequest("Passwords don't match");

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            return ServiceResult.BadRequest("New password cannot be the same as old password");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.PasswordHash = newPasswordHash;
        user.MustChangePassword = false;
        user.TempPasswordExpiresAt = null;
        
        await _dbContext.RefreshTokens.Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Revoked, true));

        await _dbContext.SaveChangesAsync();
        return ServiceResult.Success();
    }
}