using System.ComponentModel.DataAnnotations;
using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Entities;

public class ArticlesEntity : IOwnedEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }  
    public required string Content { get; set; }
    public required DateOnly PublishDate { get; set; }
    public DateOnly? ModifiedDate { get; set; }
    public required Guid OwnerId { get; set; }
    public List<LikesEntity> Likes { get; set; } = [];
    public List<CommentsEntity> Comments { get; set; } = [];
}