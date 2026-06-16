using System.Security.Claims;

namespace ContactManager.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Reads the authenticated user's id from the JWT subject claim. The owning user is
    /// always taken from the token, never from the request body or route.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        // ASP.NET maps the JWT "sub" claim to NameIdentifier by default.
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");
    }
}
