using Api.Common;
using Api.Features.Users;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "AgentOrAdmin")]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters queryParameters)
    {
        var result = await _userService.GetUsersAsync(queryParameters);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return Ok(result.Data);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "AgentOrAdmin")]
    public async Task<IActionResult> GetUser(long id)
    {
        var result = await _userService.GetUserAsync(id);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return Ok(result.Data);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.GetUserId();
        var result = await _userService.GetUserAsync(userId);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);

        if (!result.IsSuccess) return this.ToActionResult(result);

        return CreatedAtAction(nameof(GetUser), new { id = result.Data.Id }, result.Data);
    }

    [HttpPatch("{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(long id, UpdateUserRequest request)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();

        var result = await _userService.UpdateUserAsync(id, userId, userRole, request);
        if (!result.IsSuccess) return this.ToActionResult(result);

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result.IsSuccess) return this.ToActionResult(result);

        return NoContent();
    }
}