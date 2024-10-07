using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Endpoints;
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
        
        await app.CheckJwtExpiration();
        
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

    private static async Task CheckJwtExpiration(this WebApplication app)
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

    private static async Task MigrateDb(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}