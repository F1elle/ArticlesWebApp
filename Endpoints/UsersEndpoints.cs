using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users");

        group.MapGet("/{id}/articles", GetUserArticlesHandler)
             .WithSummary("Returns all user articles");
        group.MapGet("/{id}/comments", GetUserCommentsHandler)
            .WithSummary("Returns all user's comments");

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
            .Where(a => a.AuthorId == userId)
            .OrderByDescending(a => a.PublishDate)
            .Select(a => new UserArticlesResponse
            (
                a.Id,
                a.Title,
                a.Content,
                a.PublishDate,
                a.ModifiedDate,
                a.AuthorId
            )).ToListAsync();
    }

    private static async Task<List<CommentsEntity>> GetUserCommentsHandler(ArticlesDbContext dbContext,
            Guid userId)
    {
        return await dbContext.Comments.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
    }
}








