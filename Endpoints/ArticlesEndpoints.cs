using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class ArticlesEndpoints
{
    public static RouteGroupBuilder MapArticlesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("articles").RequireAuthorization();

        group.MapGet("/{articleId}", GetArticlesByIdHandler)
             .WithSummary("Get articles by id").WithName("ArticlesGetById");
        group.MapPost("/", PostArticlesHandler)
             .WithSummary("Post articles");
        group.MapPut("/{articleId}", UpdateArticlesHandler)
             .WithSummary("Edit articles");
        group.MapDelete("/{articleId}", DeleteArticlesHandler)
             .WithSummary("Delete articles by id");
        group.MapPost("/{articleId}/likes", LikesHandler)
            .WithSummary("Like articles");

        return group;
    }
    
    private static async Task<Ok<Guid>> PostArticlesHandler(ArticlesDbContext dbContext,
            string articleTitle, 
            string articleContent,
            ClaimsPrincipal author)
    {
        var article = new ArticlesEntity
        {
            Title = articleTitle,
            Content = articleContent,
            PublishDate = DateOnly.FromDateTime(DateTime.Today),
            AuthorId = author.GetUserId()
        };
        
        await dbContext.Articles.AddAsync(article);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(article.Id);
    }

    private static async Task<Results<Ok, NotFound, ForbidHttpResult>> UpdateArticlesHandler(ArticlesDbContext dbContext,
            Guid articleId, 
            string updatedTitle, 
            string updatedContent,
            ClaimsPrincipal user)
    {
        var article = await dbContext.Articles.FindAsync(articleId);
        if (article == null)
        {
            return TypedResults.NotFound();
        }

        if (article.AuthorId != user.GetUserId())
        {
            return TypedResults.Forbid();
        }
        
        article.Content = updatedContent;
        article.Title = updatedTitle;
        article.ModifiedDate = DateOnly.FromDateTime(DateTime.Today);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ArticlesEntity>, NotFound>> GetArticlesByIdHandler(ArticlesDbContext dbContext,
            Guid articleId)
    {
        var article = await dbContext.Articles
            .AsNoTracking()
            .Where(e => e.Id == articleId)
            .Include(a => a.Comments)
            .FirstOrDefaultAsync();
        
        return article is null 
            ? TypedResults.NotFound()
            : TypedResults.Ok(article);
    }

    private static async Task<NoContent> DeleteArticlesHandler(ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal user)
    {
        await dbContext.Articles
            .Where(a => a.Id == articleId && a.AuthorId == user.GetUserId())
            .ExecuteDeleteAsync();
        
        return TypedResults.NoContent();
    }

    private static async Task<Ok> LikesHandler(ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal user)
    {
        try
        {
            await dbContext.Likes
                .AsNoTracking()
                .FirstAsync(e => e.ArticleId == articleId && e.UserId == user.GetUserId());
            
            await dbContext.Likes
                .Where(l => l.ArticleId == articleId && l.UserId == user.GetUserId())
                .ExecuteDeleteAsync();
            
            return TypedResults.Ok();
        }
        catch (InvalidOperationException)
        {
            await dbContext.Likes.AddAsync(new LikesEntity(user.GetUserId(), articleId));
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok();
        }
    }
}







