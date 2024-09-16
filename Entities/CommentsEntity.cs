namespace ArticlesWebApp.Api.Entities;

public class CommentsEntity(Guid userId, Guid articleId, string content)
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = userId;
    public Guid ArticleId { get; set; } = articleId;
    public string Content { get; set; } = content;
    public DateOnly PublishedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}