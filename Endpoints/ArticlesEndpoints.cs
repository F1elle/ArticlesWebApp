using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class ArticlesEndpoints
{
    public static RouteGroupBuilder MapArticlesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("articles");

        group.MapGet("/{articleId}", GetArticlesByIdHandler)
             .WithSummary("Get articles by id").WithName("ArticlesGetById");
        group.MapPost("/", PostArticlesHandler)
             .WithSummary("Post articles");
        group.MapPut("/{articleId}", UpdateArticlesHandler)
             .WithSummary("Edit articles");
        group.MapDelete("/{articleId}", DeleteArticlesHandler)
             .WithSummary("Delete articles by id");
        group.MapPut("/{articleId}/likes", LikesHandler)
            .WithSummary("Like articles");

        return group;
    }
    
    private static async Task<Results<Ok<Guid>, ForbidHttpResult>> PostArticlesHandler(ArticlesDbContext dbContext,
            string articleTitle, 
            string articleContent,
            ClaimsPrincipal author,
            IAuthorizationService authorizationService)
    {
        var authResult = await authorizationService
            .AuthorizeAsync(author, null, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        var article = new ArticlesEntity
        {
            Title = articleTitle,
            Content = articleContent,
            PublishDate = DateOnly.FromDateTime(DateTime.Today),
            OwnerId = author.GetUserId()
        };
        
        await dbContext.Articles.AddAsync(article);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(article.Id);
    }

    private static async Task<Results<Ok, NotFound, ForbidHttpResult>> UpdateArticlesHandler(ArticlesDbContext dbContext,
            Guid articleId, 
            string updatedTitle, 
            string updatedContent,
            ClaimsPrincipal user,
            IAuthorizationService authorizationService)
    {
        var article = await dbContext.Articles.FindAsync(articleId);
        if (article == null)
        {
            return TypedResults.NotFound();
        }
        
        var authResult = await authorizationService.AuthorizeAsync(user, article, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        
        article.Content = updatedContent;
        article.Title = updatedTitle;
        article.ModifiedDate = DateOnly.FromDateTime(DateTime.Today);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ArticlesEntity>, NotFound, ForbidHttpResult>> GetArticlesByIdHandler(ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal user)
    {
        var article = await dbContext.Articles
        .AsNoTracking()
        .Where(e => e.Id == articleId)
        .Include(a => a.Comments)
        .FirstOrDefaultAsync();

        if (article == null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(article);
        
    }

    private static async Task<Results<NoContent, ForbidHttpResult>> DeleteArticlesHandler(ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal user,
            IAuthorizationService authorizationService)
    {
        var article = await dbContext.Articles.FindAsync(articleId);
        
        var authResult = await authorizationService.AuthorizeAsync(user, article, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        await dbContext.Articles
            .Where(a => a.Id == articleId)
            .ExecuteDeleteAsync();
        
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok, ForbidHttpResult>> LikesHandler(ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal user,
            IAuthorizationService authorizationService)
    {
        var authResult = await authorizationService.AuthorizeAsync(user, null, new RoleRequirement(Roles.User));
        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        try
        {
            await dbContext.Likes
                .AsNoTracking()
                .FirstAsync(e => e.ArticleId == articleId && e.OwnerId == user.GetUserId());
            
            await dbContext.Likes
                .Where(l => l.ArticleId == articleId && l.OwnerId == user.GetUserId())
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







