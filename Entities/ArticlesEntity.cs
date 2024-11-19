using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class ArticlesEntity : IOwnedEntity
{
    public Guid Id { get; init; }
    public required string Title { get; set; }  
    public required string Content { get; set; }
    public required DateOnly PublishDate { get; init; }
    public DateOnly? ModifiedDate { get; set; }
    public required Guid OwnerId { get; init; }
    public List<LikesEntity> Likes { get; set; } = [];
    public List<CommentsEntity> Comments { get; set; } = [];
}