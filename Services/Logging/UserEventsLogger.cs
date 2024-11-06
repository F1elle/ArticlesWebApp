using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Services.Logging;

public class UserEventsLogger(DbContext dbContext) : IUserEventsLogger
{
    public async Task WriteLogAsync(BaseEventEntity eventEntity)
    {
        switch (eventEntity)
        {
            case AuthEventsEntity authEvent:
                await dbContext.Set<AuthEventsEntity>().AddAsync(authEvent);
                break;
            case EventsEntity eventsEntity:
                await dbContext.Set<EventsEntity>().AddAsync(eventsEntity);
                break;
        }
    }
}
