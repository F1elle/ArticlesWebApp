using ArticlesWebApp.Api.Common;
using Microsoft.AspNetCore.Authorization;

namespace ArticlesWebApp.Api.Services;

public class RoleRequirement(Roles role) : IAuthorizationRequirement
{
    public Roles Role { get; init; } = role;
}