using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class EventsEntity(
        bool isSucceeded,
        Guid userId,
        Guid subjectId,
        Events eventType): BaseEventEntity(isSucceeded, userId)
{
    public Guid SubjectId { get; init; } = subjectId;
    public Events EventType { get; init; } = eventType;
}
