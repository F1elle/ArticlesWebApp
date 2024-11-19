using System.Security.Claims;
using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArticlesWebApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/signup", SignupEndpointHandler)
            .WithSummary("Sign up");
        group.MapPost("/login", LoginEndpointHandler)
            .WithSummary("Log In");
        group.MapPut("/changeusername", ChangeUsernameHandler)
            .WithSummary("Change username")
            .RequireAuthorization(policy =>
                policy.AddRequirements(new RoleRequirement(Roles.User)));
        group.MapPut("/changepassword", ChangePasswordHandler)
            .WithSummary("Change password")
            .RequireAuthorization(policy =>
                policy.AddRequirements(new RoleRequirement(Roles.User)));
        group.MapGet("/logout", LogOutHandler)
            .WithSummary("Log out")
            .RequireAuthorization(policy =>
                policy.AddRequirements(new RoleRequirement(Roles.User)));

        return group;
    }

    public record Request(string username, string password);
    
    private static async Task<Results<Ok, UnauthorizedHttpResult>> LoginEndpointHandler(
            Request request,
            ArticlesDbContext dbContext,
            IPasswordsHasher hasher,
            IJwtProvider jwtProvider,
            HttpContext httpContext,
            IUserEventsLogger userEventsLogger,
            IOptions<CookiesNames> cookiesNames)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == request.username);

        if (user == null) return TypedResults.Unauthorized(); // Invalid username
        
        if (!hasher.VerifyHashedPassword(user.PasswordHash, request.password))
        {
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                httpContext.User.GetTempUserId() ?? Guid.Empty,
                AuthEvents.Login));
            
            return TypedResults.Unauthorized(); // Invalid password
        }
        
        httpContext.Response.Cookies
            .Append(cookiesNames.Value.Authorized, jwtProvider.GetToken(user));
        httpContext.Response.Cookies
            .Delete(cookiesNames.Value.Anonymous);
        
        await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
            true,
            user.Id,
            AuthEvents.Login));
        
        return TypedResults.Ok();
    }
    
    private static async Task<Results<Ok, Conflict<string>, BadRequest<string>>> SignupEndpointHandler(
            Request request,
            ArticlesDbContext dbContext,
            IPasswordsHasher hasher,
            IValidator<string> validator,
            ClaimsPrincipal claimsPrincipal,
            IUserEventsLogger userEventsLogger)
    {
        var existingUser = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == request.username);
        if (existingUser != null) return TypedResults.Conflict("This username is already taken.");
        
        var validationResult = await validator.ValidateAsync(request.password);
        if (!validationResult.IsValid)
            return TypedResults.BadRequest($"{String.Join("; ", validationResult
                .Errors.Select(x => x.ErrorMessage))}"); 
        
        var userRole = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.Id == (int)Roles.User);
        
        if (userRole == null) throw new Exception("Error resolving role"); // TODO: change exception type
        
        var user = new UsersEntity(
            request.username,
            hasher.HashPassword(request.password))
        {
            Role = userRole
        };
        
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        
        await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
            true, 
            user.Id, 
            AuthEvents.Signup));
        
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ForbidHttpResult>> ChangePasswordHandler(ArticlesDbContext dbContext,
            IPasswordsHasher hasher,
            IValidator<string> validator,
            string oldPassword,
            string newPassword,
            string newPasswordRepeated,
            ClaimsPrincipal userData,
            IUserEventsLogger userEventsLogger)
    {
        var userId = userData.GetUserId();
        var user = await dbContext.Users.FindAsync(userId);

        if (user == null) return TypedResults.BadRequest("Invalid userId. Are you signed in?");

        if (!hasher.VerifyHashedPassword(user.PasswordHash, oldPassword))
        {
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                userId ?? Guid.Empty,
                AuthEvents.ChangedPassword));
            return TypedResults.Forbid();
        }

        if (newPassword != newPasswordRepeated)
            return TypedResults.BadRequest("Passwords do not match");

        var validationResult = await validator.ValidateAsync(newPassword);
        if (!validationResult.IsValid)
            return TypedResults.BadRequest("Your new password is too weak");

        user.PasswordHash = hasher.HashPassword(newPassword);
        await dbContext.SaveChangesAsync();

        await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
            true,
            user.Id,
            AuthEvents.ChangedPassword));

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, UnauthorizedHttpResult, Conflict<string>>> ChangeUsernameHandler(
            ArticlesDbContext dbContext,
            ClaimsPrincipal userData,
            Request request,
            IPasswordsHasher hasher,
            IUserEventsLogger userEventsLogger) { 
        
        var user = await dbContext.Users.FindAsync(userData.GetUserId());
        if (user == null) return TypedResults.Unauthorized();

        if (!hasher.VerifyHashedPassword(user.PasswordHash, request.password))
        {
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                userData.GetUserId() ?? Guid.Empty,
                AuthEvents.ChangedUserName));
            return TypedResults.BadRequest("Invalid password");
        }
        
        var existingUser = await dbContext.Users
            .AsNoTracking().FirstOrDefaultAsync(u => u.UserName == request.username);
        
        if (existingUser != null) return TypedResults.Conflict("This username is already taken.");
        
        user.UserName = request.username;
        await dbContext.SaveChangesAsync();
        await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
            true,
            userData.GetUserId() ?? Guid.Empty,
            AuthEvents.ChangedUserName));
        return TypedResults.Ok();
    }

    private static Ok LogOutHandler(HttpContext httpContext, 
            IOptions<CookiesNames> cookiesNames)
    {
        httpContext.Response.Cookies.Delete(cookiesNames.Value.Authorized);
        
        return TypedResults.Ok();
    }
}
