using ArticlesWebApp.Api.Common;

namespace ArticlesWebApp.Api.Services;

public class PasswordHasher
{
    public string HashPassword(string password) => 
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);

    public bool VerifyHashedPassword(string hashedPassword, string password) => 
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    
}