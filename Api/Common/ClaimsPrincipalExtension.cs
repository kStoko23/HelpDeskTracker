using System.Security.Claims;

namespace Api.Common;

public static class ClaimsPrincipalExtension
{
    public static long GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null) throw new UnauthorizedAccessException("Missing user id claim");

        return long.Parse(value);
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.Role)?.Value;
        if(value == null)  throw new UnauthorizedAccessException("Missing role claim");
        return value;
    }
}