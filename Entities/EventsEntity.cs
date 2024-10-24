using ArticlesWebApp.Api.Common;

namespace ArticlesWebApp.Api.Entities;

public class EventsEntity(
        Guid userId,
        Guid subjectId,
        Events eventType,
        bool isSucceeded
)
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; } = userId;
    public Guid SubjectId { get; init; } = subjectId;
    public Events EventType { get; init; } = eventType;
    public bool IsSucceeded { get; set; } = isSucceeded;
    public DateTime EventTime { get; init; }
    //public string? EventString { get; init; }
}
