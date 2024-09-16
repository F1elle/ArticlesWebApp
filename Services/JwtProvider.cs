using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Endpoints;
using ArticlesWebApp.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ArticlesWebApp.Api.Services;

public class JwtProvider(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;
    
    public string GetToken(UsersEntity user)
    {
        Claim[] claims = [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())];
        
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials,
            expires: DateTime.Now.AddHours(_options.ExpiresHours));
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}