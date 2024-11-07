using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Endpoints;
using ArticlesWebApp.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapEndpoints();

        app.UseAuthentication();
        app.UseAuthorization();

        await app.CheckJwtCookieExpiration();
        await app.GiveTempId();

        await app.MigrateDb();
    }

    private static void MapEndpoints(this WebApplication app)
    {
        app.MapArticlesEndpoints();
        app.MapUsersEndpoints();
        app.MapAuthEndpoints();
        app.MapCommentsEndpoints();
        app.MapAdminEndpoints();
    }

    private static async Task CheckJwtCookieExpiration(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.User.GetUserId() == null)
            {
                context.Response.Cookies.Delete("auth");
            }
            await next(context);
        });
    }

    private static async Task GiveTempId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.User.GetUserId() == null)
            {
                using var scope = app.Services.CreateScope();
                var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();
                context.Response.Cookies.Append("tempId", jwtProvider.GetTempToken());
            }
            await next(context);
        });
    }

    private static async Task MigrateDb(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
