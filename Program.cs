using ArticlesWebApp.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.ConfigureAppServices();


var app = builder.Build();

await app.Configure();

app.Run();