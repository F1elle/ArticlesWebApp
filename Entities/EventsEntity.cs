using ArticlesWebApp.Api.Common;

namespace ArticlesWebApp.Api.Entities;

public class EventsEntity
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid SubjectId { get; init; }
    public Events EventType { get; init; }
    public DateTime EventTime { get; init; }
    public string EventString { get; init; }
}
