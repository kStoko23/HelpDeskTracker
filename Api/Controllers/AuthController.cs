using Api.Common;
using Api.Features.Auth;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess) return this.ToActionResult(result);
        
        Response.Cookies.Append("refreshToken", result.Data!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/api/auth"
        });

        return Ok(new LoginResponse
        {
            AccessToken = result.Data.AccessToken,
            ExpiresAt = result.Data.ExpiresAt
        });
    }

    [HttpPost("register")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess) return this.ToActionResult(result);

        return Ok("Account created successfully");
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var rawToken = Request.Cookies["refreshToken"];
        var result = await _authService.RefreshAsync(rawToken);
        
        if (!result.IsSuccess) return this.ToActionResult(result);

        Response.Cookies.Append("refreshToken", result.Data!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/api/auth"
        });
        
        return Ok(new
        {
            accessToken = result.Data.AccessToken,
            expiresAt = result.Data.ExpiresAt
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var rawToken = Request.Cookies["refreshToken"];
        var result = await _authService.LogoutAsync(rawToken);
        
        if (!result.IsSuccess) return this.ToActionResult(result);
        
        Response.Cookies.Delete("refreshToken", new CookieOptions{ Path = "/api/auth"});
        
        return Ok();
    }
}