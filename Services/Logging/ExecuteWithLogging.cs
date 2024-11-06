using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Common;


// still in development
public class ExecuteWithEventLogging(DbContext dbContext) 
{
    public IStatusCodeHttpResult result;
    public bool isSucceeded = false;

    public async Task<IStatusCodeHttpResult> ExecWithLoggingAsync(
            Guid userId,
            Events eventType,
            Guid subjectId,
            Func<Task<IStatusCodeHttpResult>> operation)
    {
        result = await operation();
        isSucceeded = result.StatusCode < 300;

        await WriteLog(new EventsEntity(isSucceeded, userId, subjectId, eventType));
        return result;
    }

    public async Task<IStatusCodeHttpResult> ExecWithLoggingAsync(
            Guid userId, 
            AuthEvents eventType,
            Func<Task<IStatusCodeHttpResult>> operation)
    {
        result = await operation();

        isSucceeded = result.StatusCode < 300;

        await WriteLog(new AuthEventsEntity(isSucceeded, userId, eventType));
        return result;
    }

    public async Task WriteLog(BaseEventEntity eventEntity)
    {
        switch (eventEntity)
        {
            case AuthEventsEntity ae:
                await dbContext.Set<AuthEventsEntity>().AddAsync(ae);
                break;
            case EventsEntity e:
                await dbContext.Set<EventsEntity>().AddAsync(e);
                break;
        }
    }
}
