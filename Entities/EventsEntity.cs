using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class EventsEntity : BaseEventEntity
{
    //public Guid Id { get; init; }
    public Guid UserId { get; init; } //= string.Empty; //= userId;
    public Guid SubjectId { get; init; } //= subjectId;
    public Events EventType { get; init; } //= eventType;
    public bool IsSucceeded { get; set; } //= isSucceeded;
    //public DateTime EventTime { get; init; }
    //public string? EventString { get; init; }
}
