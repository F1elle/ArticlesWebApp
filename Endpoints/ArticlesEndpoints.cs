using System.Security.Claims;
using ArticlesWebApp.Api.Abstractions;
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
        var group = app.MapGroup("/articles");

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
    
    private static async Task<Results<Ok<Guid>, BadRequest<string>, UnauthorizedHttpResult>> PostArticlesHandler(ArticlesDbContext dbContext,
            IUserEventsLogger userEventsLogger,
            InputArticlesDto inputArticle,
            ClaimsPrincipal userClaims,
            IValidator<InputArticlesDto> validator)
    {
        var validationResult = await validator.ValidateAsync(inputArticle);
        var userId = userClaims.GetUserId() ?? Guid.Empty;

        if (userId == Guid.Empty)
            return TypedResults.Unauthorized();
        
        if (!validationResult.IsValid) 
        {
            return TypedResults.BadRequest($"{string.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");
        }

        var article = new ArticlesEntity
        {
            Title = inputArticle.Title,
            Content = inputArticle.Content,
            PublishDate = DateOnly.FromDateTime(DateTime.Today),
            OwnerId = userId
        };
        
        await dbContext.Articles.AddAsync(article);
        await dbContext.SaveChangesAsync();
        
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            userId,
            article.Id,
            Events.Creating));
        
        return TypedResults.Ok(article.Id);
    }

    private static async Task<Results<Ok, NotFound, ForbidHttpResult, BadRequest<string>>> UpdateArticlesHandler(
            ArticlesDbContext dbContext,
            Guid articleId, 
            InputArticlesDto inputArticle,
            ClaimsPrincipal userClaims,
            IAuthorizationService authorizationService,
            IValidator<InputArticlesDto> validator,
            IUserEventsLogger userEventsLogger)
    {
        var article = await dbContext.Articles.FindAsync(articleId);
        
        if (article == null) return TypedResults.NotFound();
        
        
        var authResult = await authorizationService.AuthorizeAsync(userClaims, article, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded)
            return TypedResults.Forbid();
        
        var validationResult = await validator.ValidateAsync(inputArticle);
        
        if (!validationResult.IsValid)
            return TypedResults
            .BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");
        
        article.Content = inputArticle.Content;
        article.Title = inputArticle.Title;
        article.ModifiedDate = DateOnly.FromDateTime(DateTime.Today);
        
        await dbContext.SaveChangesAsync();
        
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            userClaims.GetUserId() ?? Guid.Empty,
            articleId,
            Events.Updating));
        
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ArticlesEntity>, NotFound, ForbidHttpResult>> GetArticlesByIdHandler(
            ArticlesDbContext dbContext,
            Guid articleId)
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

    private static async Task<Results<NoContent, ForbidHttpResult, NotFound, BadRequest<string>>> DeleteArticlesHandler(
            ArticlesDbContext dbContext,
            Guid articleId,
            ClaimsPrincipal userClaims,
            IAuthorizationService authorizationService,
            IUserEventsLogger userEventsLogger)
    {
        var nullableArticle = await dbContext.Articles
            .AsNoTracking().FirstOrDefaultAsync(a => a.Id == articleId);
        
        if (nullableArticle is not { } article) return TypedResults.NotFound();
        
        var authResult = await authorizationService
            .AuthorizeAsync(userClaims, article, new RoleRequirement(Roles.User));
        if (!authResult.Succeeded) return TypedResults.Forbid(); 
        
        var deleted = await dbContext.Articles
            .Where(a => a.Id == articleId)
            .ExecuteDeleteAsync();

        if (deleted == 0) return TypedResults.BadRequest("Something went wrong. Nothing deleted");
        
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            userClaims.GetUserId() ?? Guid.Empty,
            articleId,
            Events.Deleting));
        
        return TypedResults.NoContent();
    }
    
    private static async Task<Results<Ok, UnauthorizedHttpResult, NotFound>> LikesHandler(ArticlesDbContext dbContext,
        Guid articleId,
        ClaimsPrincipal userClaims)
    {
        var userId = userClaims.GetUserId();
        
        if (userId is not { } uid) return TypedResults.Unauthorized();
        
        var article = await dbContext.Articles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == articleId);
        if (article is null) return TypedResults.NotFound();
        
        var deleted = await dbContext.ArticlesLikes
            .Where(l => l.PostId == articleId && l.OwnerId == uid)
            .ExecuteDeleteAsync();

        if (deleted > 0) return TypedResults.Ok();
        
        await dbContext.ArticlesLikes.AddAsync(new LikesEntity(uid, articleId));
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok();
    }
}







