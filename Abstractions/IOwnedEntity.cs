namespace ArticlesWebApp.Api.Abstractions;

public interface IOwnedEntity
{
    public Guid OwnerId { get; init; }
}