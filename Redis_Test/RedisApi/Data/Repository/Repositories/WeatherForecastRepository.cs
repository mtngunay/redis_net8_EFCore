using Microsoft.EntityFrameworkCore;
using RedisApi.Data;
using RedisApi.Data.Entity;
using RedisApi.Data.Repository.Repositories;
using StackExchange.Redis;

namespace RedisApi
{
    public class WeatherForecastRepository : Repository<WeatherForecast>
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public WeatherForecastRepository(AppDbContext context, IConnectionMultiplexer redis) : base(context, redis)
        {
            _context = context;
            _redis = redis;
        }
    }
}
