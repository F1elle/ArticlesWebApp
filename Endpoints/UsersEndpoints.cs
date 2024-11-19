using System.Security.Claims;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.DTOs;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users");
        
        group.MapGet("/{id}/articles", GetUserArticlesHandler)
             .WithSummary("Returns all user articles");
        group.MapGet("/{id}/comments", GetUserCommentsHandler)
            .WithSummary("Returns all user's comments");
        group.MapGet("/{id}/userinfo", GetUserHandler)
            .WithSummary("Returns info about user with the provided ID");
        group.MapGet("/me", GetMeHandler)
            .RequireAuthorization(policy => policy.AddRequirements(new RoleRequirement(Roles.User)))
            .WithSummary("Returns user's info");

        return group;
    }
    

    private static async Task<List<ArticlesEntity>> GetUserArticlesHandler(
            ArticlesDbContext dbContext,
            Guid userId)
    {
        return await dbContext.Articles
            .Where(a => a.OwnerId == userId)
            .OrderByDescending(a => a.PublishDate)
            .ToListAsync();
    }

    private static async Task<List<CommentsEntity>> GetUserCommentsHandler(
            ArticlesDbContext dbContext,
            Guid userId)
    {
        return await dbContext.Comments
            .AsNoTracking()
            .Where(c => c.OwnerId == userId)
            .ToListAsync();
    }

    private static async Task<Results<Ok<OutputUsersDto>, NotFound>> GetUserHandler(
            ArticlesDbContext dbContext,
            Guid userId)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Include(u => u.Role)
            .Select(u => new OutputUsersDto(
                u.UserName,
                u.RegisterDate,
                u.Role))
            .FirstOrDefaultAsync();
        
        if (user == null) return TypedResults.NotFound();

        return TypedResults.Ok(user);
    }

    private static async Task<Results<Ok<OutputUsersDto>, ProblemHttpResult>> GetMeHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal userClaims)
    {
        var nullableUid = userClaims.GetUserId();

        if (nullableUid is not { } uid)
            return TypedResults.Problem("Something went wrong. Are you logged id?");
        
        var nullableUserEntity = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == uid)
            .Include(u => u.Role)
            .Select(u => new OutputUsersDto(
                u.UserName,
                u.RegisterDate,
                u.Role))
            .FirstOrDefaultAsync();
        if (nullableUserEntity is not { } userEntity) 
            return TypedResults.Problem("Something went wrong. Are you logged id?");
        
        return TypedResults.Ok(userEntity);
    }
}








