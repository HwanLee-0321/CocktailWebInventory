using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173") // 프론트엔드 주소
            .AllowAnyMethod()
            .AllowAnyHeader());
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<TranslatorService>();
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
