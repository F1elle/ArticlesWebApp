using ArticlesWebApp.Api.Entities;

namespace ArticlesWebApp.Api.Abstractions;

public interface IJwtProvider
{
    public string GetToken(UsersEntity user);

    public string GetTempToken();
}