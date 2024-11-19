using ArticlesWebApp.Api.Entities;

namespace ArticlesWebApp.Api.DTOs;

public class OutputUsersDto (
    string username,
    DateOnly registerDate,
    RolesEntity role)
{
    public string Username { get; set; } = username;
    public DateOnly RegisterDate { get; set; } = registerDate;
    public RolesEntity Role { get; set; } = role;
}