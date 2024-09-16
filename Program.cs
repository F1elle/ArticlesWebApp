using ArticlesWebApp.Api;
using ArticlesWebApp.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.ConfigureAppServices();

var app = builder.Build();

await app.Configure();

app.Run();