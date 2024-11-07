using System.Security.Claims;
using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
        group.MapPut("/changeusername", ChangeLoginHandler)
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


    private static async Task<Results<Ok, BadRequest<string>>> LoginEndpointHandler(Request request,
        ArticlesDbContext dbContext,
        IPasswordsHasher hasher,
        IJwtProvider jwtProvider,
        HttpContext httpContext,
        IUserEventsLogger userEventsLogger)
    {
        try
        {
            var user = await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);

            if (hasher.VerifyHashedPassword(user.PasswordHash, request.password))
            {
                httpContext.Response.Cookies.Append("auth", jwtProvider.GetToken(user));
                httpContext.Response.Cookies.Delete("tempId");
                await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                    true,
                    user.Id,
                    AuthEvents.Login));
                return TypedResults.Ok();
            }

            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                httpContext.User.GetTempUserId() ?? Guid.Empty,
                AuthEvents.Login));
            return TypedResults.BadRequest("Invalid password.");
        }
        catch (InvalidOperationException)
        {
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                httpContext.User.GetTempUserId() ?? Guid.Empty,
                AuthEvents.Login));
            return TypedResults.BadRequest("Invalid username.");
        }

    }

    private static async Task<Results<Ok, BadRequest<string>>> SignupEndpointHandler(Request request,
        ArticlesDbContext dbContext,
        IPasswordsHasher hasher,
        IValidator<string> validator,
        ClaimsPrincipal claimsPrincipal,
        IUserEventsLogger userEventsLogger)
    {
        try
        {
            await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);
        }
        catch (InvalidOperationException)
        {
            var validationResult = await validator.ValidateAsync(request.password);

            if (!validationResult.IsValid)
                return TypedResults
                .BadRequest($"{String.Join("; ", validationResult
                    .Errors.Select(x => x.ErrorMessage))}");

            var user = new UsersEntity(
                request.username,
                hasher.HashPassword(request.password));
            try
            {
                await dbContext.Users.AddAsync(user);

                var role = await dbContext.Roles.FindAsync(5);

                role.Users.Add(user);

                await dbContext.SaveChangesAsync();

                await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                    true,
                    user.Id,
                    AuthEvents.Signup));

                return TypedResults.Ok();
            }
            catch (InvalidOperationException)
            {
                await dbContext.Users
                .Where(u => u.UserName == request.username)
                .ExecuteDeleteAsync();
                return TypedResults.BadRequest("There's an error occured during assigning roles.");
            }
        }

        return TypedResults.BadRequest("This username is already taken.");
    }

    private static async Task<Results<Ok, BadRequest<string>>> ChangePasswordHandler(ArticlesDbContext dbContext,
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

        if (user == null)
        {
            return TypedResults.BadRequest("Invalid userId. Are you signed in?");
        }

        if (!hasher.VerifyHashedPassword(user.PasswordHash, oldPassword))
        {
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                userId ?? Guid.Empty,
                AuthEvents.ChangedPassword));
            return TypedResults.BadRequest("Invalid Password");
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

    private static async Task<Results<Ok, BadRequest<string>>> ChangeLoginHandler(ArticlesDbContext dbContext,
            ClaimsPrincipal userData,
            Request request,
            IPasswordsHasher hasher,
            IUserEventsLogger userEventsLogger) { 
        
        var user = await dbContext.Users.FindAsync(userData.GetUserId());
        if (user == null)
        {
            return TypedResults.BadRequest("Are you signed in?");
        }

        if (!hasher.VerifyHashedPassword(user.PasswordHash, request.password))
        {
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                false,
                userData.GetUserId() ?? Guid.Empty,
                AuthEvents.ChangedUserName));
            return TypedResults.BadRequest("Invalid password");
        }

        try
        {
            await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);
        }
        catch (InvalidOperationException e)
        {
            user.UserName = request.username;
            await dbContext.SaveChangesAsync();
            await userEventsLogger.WriteLogAsync(new AuthEventsEntity(
                true,
                userData.GetUserId() ?? Guid.Empty,
                AuthEvents.ChangedUserName));
            return TypedResults.Ok();
        }
        return TypedResults.BadRequest("This username is already taken");
    }

    private static Ok LogOutHandler(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("auth");
        
        return TypedResults.Ok();
    }
}
