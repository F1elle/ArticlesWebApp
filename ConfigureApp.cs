using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseSwagger();
        app.UseSwaggerUI();

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
    }

    public static async Task MigrateDb(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}