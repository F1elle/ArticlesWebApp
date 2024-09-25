using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Services;

public class ArticlesAuthorizationHandler(IServiceScopeFactory serviceFactory) 
    : AuthorizationHandler<RoleRequirement, IOwnedEntity?>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                RoleRequirement requirement,
                IOwnedEntity? resource)
    {
        try
        {
            var userId = context.User.GetUserId();
            using var scope = serviceFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
            
            var userRole = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Role)
                .Select(r => r.Id).FirstAsync();
            
            if (userRole > (int)requirement.Role)
            {
                context.Succeed(requirement);
            }
            else if (userRole == (int)requirement.Role)
            {
                if (resource == null)
                {
                    context.Succeed(requirement);
                }
                else if (resource.OwnerId == userId)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else
            {
                context.Fail();
            }
        }
        catch (InvalidOperationException e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            context.Fail();
        }
    }
}