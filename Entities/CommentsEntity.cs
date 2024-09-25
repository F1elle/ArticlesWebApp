using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class CommentsEntity(Guid ownerId, Guid articleId, string content) : IOwnedEntity
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; } = ownerId;
    public Guid ArticleId { get; set; } = articleId;
    public string Content { get; set; } = content;
    public DateOnly PublishedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}