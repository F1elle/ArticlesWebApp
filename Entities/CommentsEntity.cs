using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class CommentsEntity(Guid ownerId, Guid articleId, string content, Guid? commentId) : IOwnedEntity
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; } = ownerId;
    public Guid ArticleId { get; init; } = articleId;
    public Guid? CommentId { get; init; } = commentId;
    public string Content { get; set; } = content;
    public List<LikesEntity> Likes { get; set; } = [];
    public List<CommentsEntity> Comments { get; set; } = [];
    public DateOnly PublishedDate { get; init; } = DateOnly.FromDateTime(DateTime.Today);
}