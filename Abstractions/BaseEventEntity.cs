namespace ArticlesWebApp.Api.Abstractions;

public abstract class BaseEventEntity
{
    public Guid Id { get; init; }
    public string Severity { get; init; } = string.Empty;
    public DateTime TimeStamp { get; init; } = DateTime.Now;
    public string? Exception { get; init; }
}
