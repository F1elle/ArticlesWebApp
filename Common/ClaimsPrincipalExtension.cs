using System.Security.Claims;

namespace ArticlesWebApp.Api.Common;

public static class ClaimsPrincipalExtension
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            return null;
        }
        return Guid.Parse(claim);
    }
}