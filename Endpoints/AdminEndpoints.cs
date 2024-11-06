using System.Security.Claims;
using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin");

        group.MapPut("/promote/{id}", PromoteUsersHandler).RequireAuthorization(policy =>
                policy.AddRequirements(new RoleRequirement(Roles.SuperAdmin)))
            .WithSummary("Promote users");
        group.MapPut("/demote/{id}", DemoteUsersHandler).RequireAuthorization(policy =>
                policy.AddRequirements(new RoleRequirement(Roles.SuperAdmin)))
            .WithSummary("Demote users");

        return group;
    }

    private static async Task<IStatusCodeHttpResult> PromoteUsersHandler(ArticlesDbContext dbContext,
            IUserEventsLogger userEventsLogger,
            Guid userId,
            ClaimsPrincipal admin)
    {
        var user = await dbContext.Users.FindAsync(userId);

        if (user is null)
            return TypedResults.BadRequest("User not found");
        
        user.Role = await dbContext.Roles.FirstAsync(r => r.Name == "Admin");
        await dbContext.SaveChangesAsync();
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            admin.GetUserId() ?? Guid.Empty,
            userId,
            Events.Promoting));
        return TypedResults.Ok();
    }

    private static async Task<IStatusCodeHttpResult> DemoteUsersHandler(ArticlesDbContext dbContext,
            IUserEventsLogger userEventsLogger,
            Guid userId,
            ClaimsPrincipal admin)
    {
        var user = await dbContext.Users.FindAsync(userId);

        if (user is null)
            return TypedResults.BadRequest("User not found");
        
        user.Role = await dbContext.Roles.FirstAsync(r => r.Name == "User");
        await dbContext.SaveChangesAsync();
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            admin.GetUserId() ?? Guid.Empty,
            userId,
            Events.Demoting));
        return TypedResults.Ok();
    }
}
