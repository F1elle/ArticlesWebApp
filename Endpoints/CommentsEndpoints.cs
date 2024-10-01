using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.DTOs;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class CommentsEndpoints
{
    public static RouteGroupBuilder MapCommentsEndpoints(this WebApplication app)
    {
        var comments = app.MapGroup("/comments");

        comments.MapGet("/{id}", GetCommentsByIdHandler)
            .WithSummary("Get comments by id");
        comments.MapPost("/", PostCommentsHandler).RequireAuthorization(policy => 
                policy.AddRequirements(new RoleRequirement(Roles.User)))
            .WithSummary("Add comments to articles");
        comments.MapPut("/{id}", UpdateCommentsHandler)
            .WithSummary("Update comments");
        comments.MapDelete("/{id}", DeleteCommentsHandler)
            .WithSummary("Delete comments");
        comments.MapPut("/{id}/likes", LikeCommentsHandler).RequireAuthorization(policy => 
                policy.AddRequirements(new RoleRequirement(Roles.User)))
            .WithSummary("Like comments");

        return comments;
    }

    private static async Task<Results<NotFound, Ok<CommentsEntity>>> GetCommentsByIdHandler(ArticlesDbContext dbContext,
            Guid commentId)
    {
        var comment = await dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        
        return comment is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(comment);
    }
    
    private static async Task<Results<Ok<Guid>, BadRequest<string>>> PostCommentsHandler(ArticlesDbContext dbContext,
            InputCommentsDto inputComment,
            ClaimsPrincipal user,
            Guid articleId,
            Guid? commentId,
            IValidator<InputCommentsDto> validator)
    {
        var validationResult = await validator.ValidateAsync(inputComment);
        
        if (!validationResult.IsValid) return TypedResults
            .BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");
        
        var comment = new CommentsEntity(user.GetUserId(), articleId, inputComment.Content, commentId);
        await dbContext.Comments.AddAsync(comment);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(comment.Id);
    }

    private static async Task<Results<NotFound, ForbidHttpResult, Ok<Guid>, BadRequest<string>>> UpdateCommentsHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId,
            InputCommentsDto inputComment,
            IAuthorizationService authorizationService,
            IValidator<InputCommentsDto> validator)
    {
        var comment = await dbContext.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return TypedResults.NotFound();
        }
        
        var authResult = await authorizationService.AuthorizeAsync(user, comment, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded) return TypedResults.Forbid();
        
        var validationResult = await validator.ValidateAsync(inputComment);
        
        if (!validationResult.IsValid) return TypedResults
            .BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");
        
        comment.Content = inputComment.Content;
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(comment.Id);
    }

    private static async Task<Results<NoContent, ForbidHttpResult>> DeleteCommentsHandler(ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId,
            IAuthorizationService authorizationService)
    {
        var comment = await dbContext.Comments.FindAsync(commentId);
        
        var authResult = await authorizationService.AuthorizeAsync(user, comment, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded) return TypedResults.Forbid();
        
        await dbContext.Comments.Where(c => c.Id == commentId)
            .ExecuteDeleteAsync();
        
        return TypedResults.NoContent();
    }

    private static async Task<Results<ForbidHttpResult, Ok>> LikeCommentsHandler(ArticlesDbContext dbContext,
        ClaimsPrincipal user,
        Guid commentId)
    {
        try
        {
            await dbContext.CommentsLikes
                .AsNoTracking()
                .FirstAsync(e => e.PostId == commentId && e.OwnerId == user.GetUserId());
            
            await dbContext.CommentsLikes
                .Where(l => l.PostId == commentId && l.OwnerId == user.GetUserId())
                .ExecuteDeleteAsync();
            
            return TypedResults.Ok();
        }
        catch (InvalidOperationException)
        {
            await dbContext.CommentsLikes.AddAsync(new LikesEntity(user.GetUserId(), commentId));
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok();
        }
    }
}













