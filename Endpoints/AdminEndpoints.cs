
using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin");

        group.MapPut("/promote/{id}", PromoteUsersHandler)
            .WithSummary("Promote users");
        group.MapPut("/demote/{id}", DemoteUsersHandler)
            .WithSummary("Demote users");

        return group;
    }

    private static async Task<Results<Ok, BadRequest<string>, ForbidHttpResult>> PromoteUsersHandler(ArticlesDbContext dbContext, 
            Guid userId,
            IAuthorizationService authorizationService,
            ClaimsPrincipal admin)
    {
        var authResult = await authorizationService
            .AuthorizeAsync(admin, null, new RoleRequirement(Roles.SuperAdmin));

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        
        var user = await dbContext.Users.FindAsync(userId);

        if (user is null)
        {
            return TypedResults.BadRequest("User not found");
        }
        
        user.Role = await dbContext.Roles.FirstAsync(r => r.Name == "Admin");
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ForbidHttpResult>> DemoteUsersHandler(ArticlesDbContext dbContext,
            Guid userId,
            ClaimsPrincipal admin,
            IAuthorizationService authorizationService)
    {
        var authResult = await authorizationService
            .AuthorizeAsync(admin, null, new RoleRequirement(Roles.SuperAdmin));

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        
        var user = await dbContext.Users.FindAsync(userId);
        
        if (user is null)
        {
            return TypedResults.BadRequest("User not found");
        }
        
        user.Role = await dbContext.Roles.FirstAsync(r => r.Name == "User");
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok();
    }
}





