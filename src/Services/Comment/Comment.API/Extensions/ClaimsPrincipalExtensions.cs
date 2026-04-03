using System;
using System.Security.Claims;

namespace Comment.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId : Guid.Empty;
    }

    public static string GetUserCode(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("user_code")
            ?? string.Empty;
    }

    public static string GetUserFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.FindFirstValue("name")
            ?? string.Empty;
    }

    public static string? GetAvatarUrl(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("avatar_url");
    }

}
