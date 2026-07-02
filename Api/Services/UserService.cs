using System.Security.Cryptography;
using Api.Common;
using Api.Data;
using Api.Entities;
using Api.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class UserService
{
    private readonly HelpDeskDbContext _dbContext;

    public UserService(HelpDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<List<UserResponse>>> GetUsersAsync(UserQueryParameters queryParameters)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            var pattern = $"%{queryParameters.Search.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Email, pattern) || EF.Functions.Like(x.Username, pattern));
        }

        if (queryParameters.Role != null) query = query.Where(x => x.Role == queryParameters.Role);
        if (queryParameters.IsActive != null) query = query.Where(x => x.IsActive == queryParameters.IsActive);

        query = queryParameters.SortBy switch
        {
            UserSortBy.Username => queryParameters.SortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.Username)
                : query.OrderByDescending(x => x.Username),

            UserSortBy.Email => queryParameters.SortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.Email)
                : query.OrderByDescending(x => x.Email),

            UserSortBy.Role => queryParameters.SortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.Role)
                : query.OrderByDescending(x => x.Role),

            UserSortBy.IsActive => queryParameters.SortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.IsActive)
                : query.OrderByDescending(x => x.IsActive),

            UserSortBy.CreatedAt => queryParameters.SortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.CreatedAt)
                : query.OrderByDescending(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var page = Math.Max(queryParameters.Page, 1);
        var pageSize = Math.Clamp(queryParameters.PageSize, 1, 100);

        var users = await query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserResponse(x.Id, x.Email, x.Username, x.Role, x.IsActive))
            .ToListAsync();

        return ServiceResult<List<UserResponse>>.Success(users);
    }

    public async Task<ServiceResult<UserDetailResponse>> GetUserAsync(long id)
    {
        var query = _dbContext.Users.AsNoTracking().Where(x => x.Id == id);

        var user = await query.Select(x => new UserDetailResponse(x.Id, x.Username, x.Email, x.Role, x.IsActive,
                x.CreatedAt, x.CreatedTickets.Count, x.AssignedTickets.Count, x.Comments.Count,
                x.CreatedTickets.OrderByDescending(t => t.CreatedAt)
                    .Select(t => (DateTime?)t.CreatedAt)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync();

        if (user == null) return ServiceResult<UserDetailResponse>.NotFound("User not found");

        return ServiceResult<UserDetailResponse>.Success(user);
    }

    public async Task<ServiceResult<CreateUserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLower();

        if (await _dbContext.Users.AnyAsync(x => x.Email == email))
            return ServiceResult<CreateUserResponse>.Conflict("Email already taken");

        var temporaryPassword = GenerateTemporaryPassword();

        var user = new User
        {
            Email = email,
            Username = request.Username.Trim(),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            MustChangePassword = true,
            TempPasswordExpiresAt = DateTime.UtcNow.AddDays(1),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<CreateUserResponse>.Success(new CreateUserResponse(user.Id, user.Email, user.Username,
            user.Role, temporaryPassword, user.TempPasswordExpiresAt.Value));
    }

    public async Task<ServiceResult> UpdateUserAsync(long id, long currentUserId, UserRole currentUserRole,
        UpdateUserRequest request)
    {
        var user = await _dbContext.Users.FindAsync(id);

        if (user == null) return ServiceResult.NotFound("User not found");

        var isSelfUpdate = user.Id == currentUserId;

        if (currentUserRole != UserRole.Admin && !isSelfUpdate)
            return ServiceResult.Forbidden("You do not have permission to update this user");

        if (request.Role.HasValue && currentUserRole != UserRole.Admin)
            return ServiceResult.Forbidden("You do not have permission to change user role");

        if (request.Username != null) user.Username = request.Username.Trim();

        if (request.Email != null)
        {
            var email = request.Email.Trim().ToLower();

            if (await _dbContext.Users.AnyAsync(x => x.Email == email && x.Id != id))
                return ServiceResult.Conflict("Email already taken");

            user.Email = email;
        }

        if (request.Role.HasValue && currentUserRole == UserRole.Admin) user.Role = request.Role.Value;

        await _dbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteUserAsync(long id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null) return ServiceResult.NotFound("User not found");

        user.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return ServiceResult.Success();
    }

    private static string GenerateTemporaryPassword()
    {
        var randomNumber = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}