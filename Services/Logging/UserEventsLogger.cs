using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Services.Logging;

public class UserEventsLogger(ArticlesDbContext dbContext) : IUserEventsLogger
{
    public async Task WriteLogAsync(BaseEventEntity eventEntity)
    {
        switch (eventEntity)
        {
            case AuthEventsEntity authEvent:
                await dbContext.AuthEvents.AddAsync(authEvent);
                await dbContext.SaveChangesAsync();
                break;
            case EventsEntity eventsEntity:
                await dbContext.Events.AddAsync(eventsEntity);
                await dbContext.SaveChangesAsync();
                break;
        }
    }
}
