using ArticlesWebApp.Api.Common;

namespace ArticlesWebApp.Api.Entities;

public class AuthEventsEntity
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; } 
    public AuthEvents EventType { get; init; }
    public DateTime EventTime { get; init; }
    public string EventString { get; init; }
}