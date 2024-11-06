using System.Security.Claims;

namespace ArticlesWebApp.Api.Common;

public static class ClaimsPrincipalExtension
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        return claim is null
            ? null
            : Guid.Parse(claim);
    }

    public static Guid? GetTempUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.Anonymous);
        
        return claim is null
            ? null
            : Guid.Parse(claim);
    }
}