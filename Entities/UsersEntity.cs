namespace ArticlesWebApp.Api.Entities;

public class UsersEntity(string userName, string passwordHash)
{
    public Guid Id { get; init; }
    public string UserName { get; set; } = userName;
    public string PasswordHash { get; set; } = passwordHash;
    public DateOnly RegisterDate { get; init; } = DateOnly.FromDateTime(DateTime.Today);
    public List<ArticlesEntity> Articles { get; set; } = [];
    public List<LikesEntity> Likes { get; set; } = [];
    public List<CommentsEntity> Comments { get; set; } = [];
    public RolesEntity Role { get; set; } 
}