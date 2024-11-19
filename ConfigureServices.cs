using System.Text;
using ArticlesWebApp.Api.Abstractions;
using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Data;
using ArticlesWebApp.Api.DTOs;
using ArticlesWebApp.Api.Services;
using ArticlesWebApp.Api.Services.Logging;
using ArticlesWebApp.Api.Services.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace ArticlesWebApp.Api;

public static class ConfigureServices
{
    public static void ConfigureAppServices(this WebApplicationBuilder builder)
    {
        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // signalr
        builder.Services.AddSignalR();

        // DbContext
        builder.Services.AddDbContext<ArticlesDbContext>();

        // Auth features
        builder.Services.AddScoped<IJwtProvider, JwtProvider>();
        builder.Services.AddScoped<IPasswordsHasher, PasswordsHasher>();
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
        builder.ConfigureAuth();
        
        // Cookies for auth
        builder.Services.Configure<CookiesNames>(
            builder.Configuration.GetSection(nameof(CookiesNames)));

        // Validation
        builder.Services.AddScoped<IValidator<InputArticlesDto>, ArticlesValidator>();
        builder.Services.AddScoped<IValidator<InputCommentsDto>, CommentsValidator>();
        builder.Services.AddScoped<IValidator<string>, PasswordsValidator>();

        // Logging
        builder.Services.AddScoped<IUserEventsLogger, UserEventsLogger>();
    }

    private static void ConfigureAuth(this WebApplicationBuilder builder)
    {
        var cookiesNames = builder.Configuration.GetSection(nameof(CookiesNames)).Get<CookiesNames>();
        var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions!.SecretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[cookiesNames!.Authorized];

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();
        
        builder.Services.AddSingleton<IAuthorizationHandler, ResourceAuthorizationHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();
    }
}
