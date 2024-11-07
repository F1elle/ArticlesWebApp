using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class AuthEventsEntity(
    bool isSucceeded,
    Guid userId,
    AuthEvents eventType) : BaseEventEntity(isSucceeded, userId)
{
    public AuthEvents EventType { get; init; } = eventType;
}
