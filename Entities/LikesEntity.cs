namespace ArticlesWebApp.Api.Entities;

public class LikesEntity(Guid userId, Guid articleId)
{
    public Guid UserId { get; set; } = userId;
    public Guid ArticleId { get; set; } = articleId;
}