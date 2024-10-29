using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;

namespace ArticlesWebApp.Api.Services.Logging;


public class AuthEventsLogSink(
    DbContext dbContext,
    IHttpContextAccessor httpContextAccessor
    ) : BaseLogSink<AuthEventsEntity>(dbContext, httpContextAccessor)
{
    protected override AuthEventsEntity CreateLog(LogEvent logEvent)
    {
        var httpContext = HttpContextAccessor.HttpContext;
        httpContext.Request.Cookies.TryGetValue("tempId", out var tempIdString);
        var tempId = Guid.Parse(tempIdString ?? string.Empty);
        var authEventType = httpContext.Request.Path.ToString().ToLower() switch
        {
            var s when s.Contains("login") => AuthEvents.Login,
            var s when s.Contains("signup") => AuthEvents.Signup,
            var s when s.Contains("changeusername") => AuthEvents.ChangedUserName,
            var s when s.Contains("changepassword") => AuthEvents.ChangedPassword
        };
        return new AuthEventsEntity {
            Severity = logEvent.Level.ToString(),
            Exception = logEvent.Exception?.ToString(),
            UserId = httpContext.User.GetUserId() ?? tempId,
            IsSucceeded = httpContext.Response.StatusCode == 200,
            EventType = authEventType
        };
    }

}