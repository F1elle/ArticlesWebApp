namespace ArticlesWebApp.Api.Common;

public enum Roles
{
    // integers for roles are linked to permissions. user role is their highest permission + 1
    // if you find it strange look at RolesConfiguration.cs and I hope you'll find it to be an interesting solution
    User = 5, 
    Admin = 7,
    SuperAdmin = 8
}