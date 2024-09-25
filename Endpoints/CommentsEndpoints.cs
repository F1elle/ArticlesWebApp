using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
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
        comments.MapPost("/", PostCommentsHandler)
            .WithSummary("Add comments to articles");
        comments.MapPut("/{id}", UpdateCommentsHandler)
            .WithSummary("Update comments");
        comments.MapDelete("/{id}", DeleteCommentsHandler)
            .WithSummary("Delete comments");

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
    
    private static async Task<Results<Ok<Guid>, ForbidHttpResult>> PostCommentsHandler(ArticlesDbContext dbContext,
            string commentContent,
            ClaimsPrincipal user,
            Guid articleId,
            IAuthorizationService authorizationService)
    {
        var authResult = await authorizationService.AuthorizeAsync(user, null, new RoleRequirement(Roles.User));
        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        
        var comment = new CommentsEntity(user.GetUserId(), articleId, commentContent);
        await dbContext.Comments.AddAsync(comment);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(comment.Id);
        
        // add verification to comments
    }

    private static async Task<Results<NotFound, ForbidHttpResult, Ok<Guid>>> UpdateCommentsHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId,
            string updatedCommentContent,
            IAuthorizationService authorizationService)
    {
        var comment = await dbContext.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return TypedResults.NotFound();
        }
        
        var authResult = await authorizationService.AuthorizeAsync(user, comment, new RoleRequirement(Roles.User));

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        
        comment.Content = updatedCommentContent;
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

        if (!authResult.Succeeded)
        {
            return TypedResults.Forbid();
        }
        
        await dbContext.Comments.Where(c => c.Id == commentId)
            .ExecuteDeleteAsync();
        
        return TypedResults.NoContent();
    }
}













