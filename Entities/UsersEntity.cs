namespace ArticlesWebApp.Api.Entities;

public class UsersEntity(string userName, string passwordHash)
{
    public Guid Id { get; set; }
    public string UserName { get; private set; } = userName;
    public string PasswordHash { get; private set; } = passwordHash;
    public DateOnly RegisterDate { get; private set; } = DateOnly.FromDateTime(DateTime.Today);
    public List<ArticlesEntity> Articles { get; set; } = [];
    public List<LikesEntity> Likes { get; set; } = [];
    public List<CommentsEntity> Comments { get; set; } = [];
}