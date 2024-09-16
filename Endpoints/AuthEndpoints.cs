using System.Security.Principal;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("auth");

        group.MapPost("/signup", SignupEndpointHandler)
            .WithSummary("Sign up");
        group.MapPost("/login", LoginEndpointHandler)
            .WithSummary("Log In");

        return group;
    }

    public record Request(string username, string password);
    

    private static async Task<Results<Ok, BadRequest<string>>> LoginEndpointHandler(Request request, 
        ArticlesDbContext dbContext,
        PasswordHasher hasher,
        JwtProvider jwtProvider,
        HttpContext httpContext)
    {
        try
        {
            var user = await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);

            if (hasher.VerifyHashedPassword(user.PasswordHash, request.password))
            {
                httpContext.Response.Cookies.Append("auth", jwtProvider.GetToken(user));
                return TypedResults.Ok();
            }
            return TypedResults.BadRequest("Invalid password.");
        }
        catch (InvalidOperationException)
        {
            return TypedResults.BadRequest("Invalid username.");
        }
        
    }
    
    private static async Task<Results<Ok, BadRequest<string>>> SignupEndpointHandler(Request request,
        ArticlesDbContext dbContext,
        PasswordHasher hasher)
    {
        try
        {
            await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);
        }
        catch (InvalidOperationException)
        {
            await dbContext.Users.AddAsync(new UsersEntity(
                request.username,
                hasher.HashPassword(request.password)));
        
            await dbContext.SaveChangesAsync();

            return TypedResults.Ok();
        }
        
        return TypedResults.BadRequest("This username is already taken.");
        
    }
}