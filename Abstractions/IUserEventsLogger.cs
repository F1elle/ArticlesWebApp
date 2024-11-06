namespace ArticlesWebApp.Api.Abstractions;

public interface IUserEventsLogger
{
    public Task WriteLogAsync(BaseEventEntity eventEntity);
}
