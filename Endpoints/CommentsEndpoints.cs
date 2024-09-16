using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class CommentsEndpoints
{
    public static RouteGroupBuilder MapCommentsEndpoints(this WebApplication app)
    {
        var comments = app.MapGroup("/comments").RequireAuthorization();

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
    
    private static async Task<Ok<Guid>> PostCommentsHandler(ArticlesDbContext dbContext,
            string commentContent,
            ClaimsPrincipal user,
            Guid articleId)
    {
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
            string updatedCommentContent)
    {
        var comment = await dbContext.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return TypedResults.NotFound();
        }

        if (comment.UserId != user.GetUserId())
        {
            return TypedResults.Forbid();
        }
        
        comment.Content = updatedCommentContent;
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(comment.Id);
    }

    private static async Task<NoContent> DeleteCommentsHandler(ArticlesDbContext dbContext,
            ClaimsPrincipal user,
            Guid commentId)
    {
        await dbContext.Comments.Where(c => c.Id == commentId && c.UserId == user.GetUserId())
            .ExecuteDeleteAsync();
        
        return TypedResults.NoContent();
    }
}













