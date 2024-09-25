using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users");//.RequireAuthorization(policy => 
            //policy.AddRequirements(new RoleRequirement(Roles.User)));

        group.MapGet("/{id}/articles", GetUserArticlesHandler)
             .WithSummary("Returns all user articles");
        group.MapGet("/{id}/comments", GetUserCommentsHandler)
            .WithSummary("Returns all user's comments");
        group.MapGet("/role", GetRoleHandler)
            .WithSummary("Returns user's role");

        return group;
    }

    public record UserArticlesResponse(
        Guid Id,
        string Title,
        string Content,
        DateOnly PublishDate,
        DateOnly? ModifiedDate,
        Guid AuthorId);

    private static async Task<List<UserArticlesResponse>> GetUserArticlesHandler(ArticlesDbContext dbContext,
            Guid userId)
    {
        return await dbContext.Articles
            .Where(a => a.OwnerId == userId)
            .OrderByDescending(a => a.PublishDate)
            .Select(a => new UserArticlesResponse
            (
                a.Id,
                a.Title,
                a.Content,
                a.PublishDate,
                a.ModifiedDate,
                a.OwnerId
            )).ToListAsync();
    }

    private static async Task<List<CommentsEntity>> GetUserCommentsHandler(ArticlesDbContext dbContext,
            Guid userId)
    {
        return await dbContext.Comments.AsNoTracking().Where(c => c.OwnerId == userId).ToListAsync();
    }

    // this route is only for testing. it will be removed after adding proper userinfo page
    private static async Task<Ok<RolesEntity>> GetRoleHandler(ArticlesDbContext dbContext,
        ClaimsPrincipal user)
    {
        var role = await dbContext.Users
            .Where(u => u.Id == user.GetUserId())
            .Select(u => u.Role)
            .FirstOrDefaultAsync();
        return TypedResults.Ok(role);
    }
}








