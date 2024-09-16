using System.ComponentModel.DataAnnotations;

namespace ArticlesWebApp.Api.Entities;

public class ArticlesEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }  
    public required string Content { get; set; }
    public required DateOnly PublishDate { get; set; }
    public DateOnly? ModifiedDate { get; set; }
    public required Guid AuthorId { get; set; }
    public List<LikesEntity> Likes { get; set; } = [];
    public List<CommentsEntity> Comments { get; set; } = [];
}