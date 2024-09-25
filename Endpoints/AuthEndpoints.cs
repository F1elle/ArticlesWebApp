using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
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
        IPasswordHasher hasher,
        IJwtProvider jwtProvider,
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
        IPasswordHasher hasher)
    {
        try
        {
            await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);
        }
        catch (InvalidOperationException)
        {
            var user = new UsersEntity(
                request.username,
                hasher.HashPassword(request.password));
            try
            {
                await dbContext.Users.AddAsync(user);
                
                var role = await dbContext.Roles.FindAsync(5);
                
                role.Users.Add(user);
                
                await dbContext.SaveChangesAsync();
            
                return TypedResults.Ok();
            }
            catch (InvalidOperationException)
            {
                return TypedResults.BadRequest("There's an error occured during assigning roles.");
            }
        }
        
        return TypedResults.BadRequest("This username is already taken.");
        
    }
}