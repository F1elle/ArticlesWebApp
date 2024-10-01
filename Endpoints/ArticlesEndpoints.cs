using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.DTOs;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class ArticlesEndpoints
{
    public static RouteGroupBuilder MapArticlesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("articles");

        group.MapGet("/{articleId}", GetArticlesByIdHandler).AllowAnonymous()
             .WithSummary("Get articles by id");
        group.MapPost("/", PostArticlesHandler).RequireAuthorization(policy => 
                policy.AddRequirements(new RoleRequirement(Roles.User)))
             .WithSummary("Post articles");
        group.MapPut("/{articleId}", UpdateArticlesHandler)
             .WithSummary("Edit articles");
        group.MapDelete("/{articleId}", DeleteArticlesHandler)
             .WithSummary("Delete articles by id");
        group.MapPut("/{articleId}/likes", LikesHandler).RequireAuthorization(policy =>
                policy.AddRequirements(new RoleRequirement(Roles.User)))
            .WithSummary("Like articles");

        return group;
    }
    
    private static async Task<Results<Ok<Guid>, BadRequest<string>>> PostArticlesHandler(ArticlesDbContext dbContext,
            InputArticlesDto inputArticle,
            ClaimsPrincipal author,
            IValidator<InputArticlesDto> validator)
    {
        var validationResult = await validator.ValidateAsync(inputArticle);
        
        if (!validationResult.IsValid) return TypedResults
            .BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");
        
        var article = new ArticlesEntity
        {
            Title = inputArticle.Title,
            Content = inputArticle.Content,
            PublishDate = DateOnly.FromDateTime(DateTime.Today),
            OwnerId = author.GetUserId()
        };
        
        await dbContext.Articles.AddAsync(article);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(article.Id);
    }

    private static async Task<Results<Ok, NotFound, ForbidHttpResult, BadRequest<string>>> UpdateArticlesHandler(ArticlesDbContext dbContext,
            Guid articleId, 
            InputArticlesDto inputArticle,
            ClaimsPrincipal user,
            IAuthorizationService authorizationService,
            IValidator<InputArticlesDto> validator)
    {
        var article = await dbContext.Articles.FindAsync(articleId);
        if (article == null)
        {
            return TypedResults.NotFound();
        }
        
        var authResult = await authorizationService.AuthorizeAsync(user, article, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded) return TypedResults.Forbid();
        
        var validationResult = await validator.ValidateAsync(inputArticle);
        
        if (!validationResult.IsValid) return TypedResults
            .BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");
        
        article.Content = inputArticle.Content;
        article.Title = inputArticle.Title;
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

        if (!authResult.Succeeded) return TypedResults.Forbid();
        
        await dbContext.Articles
            .Where(a => a.Id == articleId)
            .ExecuteDeleteAsync();
        
        return TypedResults.NoContent();
    }

    private static async Task<Ok> LikesHandler(ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal user)
    {
        try
        {
            await dbContext.ArticlesLikes
                .AsNoTracking()
                .FirstAsync(e => e.PostId == articleId && e.OwnerId == user.GetUserId());
            
            await dbContext.ArticlesLikes
                .Where(l => l.PostId == articleId && l.OwnerId == user.GetUserId())
                .ExecuteDeleteAsync();
            
            return TypedResults.Ok();
        }
        catch (InvalidOperationException)
        {
            await dbContext.ArticlesLikes.AddAsync(new LikesEntity(user.GetUserId(), articleId));
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok();
        }
    }
}







