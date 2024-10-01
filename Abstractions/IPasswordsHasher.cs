namespace ArticlesWebApp.Api.Abstractions;

public interface IPasswordsHasher
{
    public string HashPassword(string password);
    public bool VerifyHashedPassword(string hashedPassword, string password);
}