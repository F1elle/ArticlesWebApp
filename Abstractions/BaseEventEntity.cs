namespace ArticlesWebApp.Api.Abstractions;

public abstract class BaseEventEntity(bool isSucceeded, Guid userId)
{
    public Guid Id { get; init; }
    public DateTime TimeStamp { get; init; } = DateTime.Now;
    public bool IsSucceeded { get; init; } = isSucceeded;
    public Guid UserId { get; init; } = userId;
}
