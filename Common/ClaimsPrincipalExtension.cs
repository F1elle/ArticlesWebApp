using System.Security.Claims;

namespace ArticlesWebApp.Api.Common;

public static class ClaimsPrincipalExtension
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            throw new InvalidOperationException("Invalid UserId"); //Exception("Invalid UserId");
        }
        return Guid.Parse(claim);
    }
}