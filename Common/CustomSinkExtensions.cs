using ArticlesWebApp.Api.Services.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Configuration;

namespace ArticlesWebApp.Api.Common;

public static class CustomSinkExtensions
{
    public static LoggerConfiguration EventsLogDatabase(
            this LoggerSinkConfiguration loggerConfiguration,
            DbContext dbContext,
            IHttpContextAccessor httpContextAccessor)
    {
        return loggerConfiguration.Sink(
            new EventsLogSink(dbContext, httpContextAccessor));
    }

    public static LoggerConfiguration AuthEventsLogDatabase(
            this LoggerSinkConfiguration loggerConfiguration,
            DbContext dbContext,
            IHttpContextAccessor httpContextAccessor)
    {
        return loggerConfiguration.Sink(
            new AuthEventsLogSink(dbContext, httpContextAccessor));
    }
}