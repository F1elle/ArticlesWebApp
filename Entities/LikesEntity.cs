using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class LikesEntity(Guid ownerId, Guid articleId) : IOwnedEntity
{
    public Guid OwnerId { get; set; } = ownerId;
    public Guid ArticleId { get; set; } = articleId;
}