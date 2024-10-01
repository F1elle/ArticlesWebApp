using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class LikesEntity(Guid ownerId, Guid postId) : IOwnedEntity
{
    public Guid OwnerId { get; set; } = ownerId;
    public Guid PostId { get; set; } = postId;
}