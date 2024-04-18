using Microsoft.EntityFrameworkCore;
using RedisApi.Data;
using RedisApi.Data.Entity;
using RedisApi;
using RedisApi.Data.Repository.Interface;
using RedisApi.Data.Repository.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

builder.Services.AddScoped<IRepository<WeatherForecast>, WeatherForecastRepository>();

builder.Services.AddSingleton<ConnectionMultiplexer>(provider =>
{
    return ConnectionMultiplexer.Connect("localhost:6379,password=your_password,ssl=False,abortConnect=False");
});

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
