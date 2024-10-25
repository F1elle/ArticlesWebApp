using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;

namespace ArticlesWebApp.Api.Services.Logging;

public class EventsLogSink(
    DbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : BaseLogSink<EventsEntity>(dbContext, httpContextAccessor)
{
    protected override EventsEntity CreateLog(LogEvent logEvent)
    {
        var httpContext = HttpContextAccessor.HttpContext;

        var eventType = httpContext.Request.Method switch
        {
            "POST" => Events.Creating,
            "PUT" => httpContext.Request.Path.ToString().ToLower() switch
            {
                var s when s.Contains("/promote") => Events.Promoting,
                var s when s.Contains("/demote") => Events.Demoting,
                _ => Events.Updating,
            },
            "DELETE" => Events.Deleting
        };

        return new EventsEntity
        {
            Severity = logEvent.Level.ToString(),
            Exception = logEvent.Exception?.ToString(),
            UserId = httpContext.User.GetUserId() ?? Guid.Empty,
            IsSucceeded = httpContext.Response.StatusCode == 200,
            EventType = eventType,
            SubjectId = Guid.Parse(httpContext.Request.Path.ToString().Split('/').Last())
        };
    }
}
