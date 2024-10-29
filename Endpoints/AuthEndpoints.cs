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
        IPasswordsHasher hasher,
        IValidator<string> validator)
    {
        try
        {
            await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);
        }
        catch (InvalidOperationException)
        {
            var validationResult = await validator.ValidateAsync(request.password);

            if (!validationResult.IsValid) return TypedResults
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
            ClaimsPrincipal userData)
    {
        var userId = userData.GetUserId();
        var user = await dbContext.Users.FindAsync(userId);

        if (user == null)
            return TypedResults.BadRequest("Invalid userId");

        if (!hasher.VerifyHashedPassword(user.PasswordHash, oldPassword))
            return TypedResults.BadRequest("Invalid Password");

        if (newPassword != newPasswordRepeated)
            return TypedResults.BadRequest("Passwords do not match");

        var validationResult = await validator.ValidateAsync(newPassword);
        if (!validationResult.IsValid)
            return TypedResults.BadRequest("Your new password is too weak");

        user.PasswordHash = hasher.HashPassword(newPassword);
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>>> ChangeLoginHandler(ArticlesDbContext dbContext,
            ClaimsPrincipal userData,
            Request request,
            IPasswordsHasher hasher)
    {
        var user = await dbContext.Users.FindAsync(userData.GetUserId());
        if (user == null)
            return TypedResults.BadRequest("Invalid userId");

        if (!hasher.VerifyHashedPassword(user.PasswordHash, request.password))
            return TypedResults.BadRequest("Invalid password");

        try
        {
            await dbContext.Users.AsNoTracking().FirstAsync(u => u.UserName == request.username);
        }
        catch (InvalidOperationException e)
        {
            user.UserName = request.username;
            await dbContext.SaveChangesAsync();
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
