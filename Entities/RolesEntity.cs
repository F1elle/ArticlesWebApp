using ArticlesWebApp.Api.Common;

namespace ArticlesWebApp.Api.Entities;

public class RolesEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Permissions> Permissions { get; set; } = [];
    public List<UsersEntity> Users { get; set; } = [];
}