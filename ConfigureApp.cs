using System.IdentityModel.Tokens.Jwt;
using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        
        await app.DeleteAuthCookieIfInvalid();
        await app.GiveTempIdCookie();
        
        app.MapEndpoints();

        app.UseAuthentication();
        app.UseAuthorization();

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

    private static async Task DeleteAuthCookieIfInvalid(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            using var scope = app.Services.CreateScope();
            var cookies = scope.ServiceProvider.GetRequiredService<IOptions<CookiesNames>>();
            var cookie = context.Request.Cookies
                .FirstOrDefault(x => x.Key == cookies.Value.Authorized);

            if (cookie.Value != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(cookie.Value);
                if (token.ValidTo < DateTime.Now)
                {
                    context.Response.Cookies.Delete(cookies.Value.Authorized);
                }
            }
            
            await next(context);
        });
    }

    private static async Task GiveTempIdCookie(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            using var scope = app.Services.CreateScope();
            var cookies = scope.ServiceProvider.GetRequiredService<IOptions<CookiesNames>>();
            var authCookie = context.Request.Cookies
                .FirstOrDefault(x => x.Key == cookies.Value.Authorized);
            if (authCookie.Value == null)
            {
                var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();
                context.Response.Cookies.Append(cookies.Value.Anonymous, jwtProvider.GenerateTempToken());
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
