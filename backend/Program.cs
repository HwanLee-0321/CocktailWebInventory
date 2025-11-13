using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<Translator>();
builder.Services.AddHttpClient("TheCocktailDbClient");
builder.Services.AddSingleton(sp => new KoCacheManager(Constants.koFilePath));
builder.Services.AddSingleton(sp => new EnCacheManager(Constants.enFilePath));
builder.Services.AddScoped<CocktailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
