using ArticlesWebApp.Api.Abstractions;

namespace ArticlesWebApp.Api.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => 
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);

    public bool VerifyHashedPassword(string hashedPassword, string password) => 
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    
}