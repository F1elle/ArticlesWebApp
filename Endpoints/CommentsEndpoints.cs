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

    private static async Task<Results<NotFound, Ok<CommentsEntity>>> GetCommentsByIdHandler(
            ArticlesDbContext dbContext,
            Guid commentId)
    {
        var comment = await dbContext.Comments.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == commentId);
        
        return comment is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(comment);
    }
    
    private static async Task<Results<Ok<Guid>, BadRequest<string>, ForbidHttpResult>> PostCommentsHandler(
            ArticlesDbContext dbContext,
            InputCommentsDto inputComment,
            ClaimsPrincipal user,
            Guid articleId,
            Guid? commentId,
            IValidator<InputCommentsDto> validator,
            IUserEventsLogger userEventsLogger)
    {
        var validationResult = await validator.ValidateAsync(inputComment);
        
        if (!validationResult.IsValid) return TypedResults
            .BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}");

        var userId = user.GetUserId();
        if (userId is not { } uid) return TypedResults.Forbid();
        
        var comment = new CommentsEntity(uid, articleId, inputComment.Content, commentId);
        await dbContext.Comments.AddAsync(comment);
        await dbContext.SaveChangesAsync();
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            uid,
            comment.Id,
            Events.Creating));
        return TypedResults.Ok(comment.Id);
    }

    private static async Task<Results<NotFound, ForbidHttpResult, Ok<Guid>, BadRequest<string>>> UpdateCommentsHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId,
            InputCommentsDto inputComment,
            IAuthorizationService authorizationService,
            IValidator<InputCommentsDto> validator,
            IUserEventsLogger userEventsLogger)
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
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            user.GetUserId() ?? Guid.Empty,
            commentId,
            Events.Updating));
        return TypedResults.Ok(comment.Id);
    }

    private static async Task<Results<NoContent, ForbidHttpResult, NotFound>> DeleteCommentsHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId,
            IAuthorizationService authorizationService,
            IUserEventsLogger userEventsLogger)
    {
        var nullableComment = await dbContext.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == commentId);
        
        if (nullableComment is not {} comment) return TypedResults.NotFound();
        
        var authResult = await authorizationService.AuthorizeAsync(user, comment, new RoleRequirement(Roles.User));
        if (!authResult.Succeeded) return TypedResults.Forbid();
        
        var deleted = await dbContext.Comments.Where(c => c.Id == commentId)
            .ExecuteDeleteAsync();

        if (deleted == 0) return TypedResults.NotFound();
        
        await userEventsLogger.WriteLogAsync(new EventsEntity(
            true,
            user.GetUserId() ?? Guid.Empty,
            commentId,
            Events.Deleting));
        
        return TypedResults.NoContent();
    }

    private static async Task<Results<ForbidHttpResult, Ok, NotFound>> LikeCommentsHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId)
    {
        var userId = user.GetUserId();
        if (userId is not { } uid) return TypedResults.Forbid();
        
        var comment = await dbContext.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == commentId);
        
        if (comment is null) return TypedResults.NotFound();

        var deleted = await dbContext.CommentsLikes
            .Where(e => e.PostId == commentId && e.OwnerId == uid)
            .ExecuteDeleteAsync();
        
        if (deleted > 0) return TypedResults.Ok();
        
        await dbContext.CommentsLikes.AddAsync(new LikesEntity(uid, commentId));
        await dbContext.SaveChangesAsync();
        
        return TypedResults.Ok();
    }
}













