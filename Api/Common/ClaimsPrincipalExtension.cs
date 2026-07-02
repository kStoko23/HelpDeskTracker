using System.Security.Claims;
using Api.Entities;

namespace Api.Common;

public static class ClaimsPrincipalExtension
{
    public static long GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Invalid user id claim.");

        return userId;
    }

    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);

        if (!Enum.TryParse<UserRole>(value, out var role))
            throw new UnauthorizedAccessException("Invalid user role claim.");

        return role;
    }
}