using Microsoft.EntityFrameworkCore;
using RedisApi.Data;
using RedisApi.Data.Entity;
using RedisApi;
using RedisApi.Data.Repository.Interface;
using RedisApi.Data.Repository.Repositories;
using StackExchange.Redis;
using RedisApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );




// IConnectionMultiplexer'ý DI konteynerine ekleyin
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse("localhost:6379", true);
    return ConnectionMultiplexer.Connect(configuration);
});

// CacheService ve WeatherForecastRepository'yi DI konteynerine ekleyin
builder.Services.AddSingleton<CacheService>();
builder.Services.AddScoped<IRepository<WeatherForecast>, WeatherForecastRepository>();

// Diðer hizmetlerinizi eklemeye devam edin


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
