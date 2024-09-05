using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RedisApi.Data.Repository.Interface;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisApi.Data.Repository.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _cache;
        private readonly string _cacheKeyPrefix;

        public Repository(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
            _cacheKeyPrefix = typeof(T).Name + ":";
        }

        public async Task<List<T>> GetAllAsync()
        {
            string cacheKey = _cacheKeyPrefix + "All";

            try
            {
                // Redis cache'ten veri almayı deneyin
                var cachedData = await _cache.StringGetAsync(cacheKey);
                if (!cachedData.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<List<T>>(cachedData);
                }

                // Redis cache'te veri bulunamazsa, veritabanından veriyi çekin
                var data = await _context.Set<T>().Take(5000).ToListAsync();

                // Veriyi Redis cache'ine ekleyin ve TTL ayarlayın
                await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(data), TimeSpan.FromMinutes(5));

                return data;
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Error retrieving data: {ex.Message}");
                // Hata durumunda veritabanından veriyi döndürün
                return await _context.Set<T>().Take(5000).ToListAsync();
            }
        }

        public async Task<T> GetByIdAsync(int id)
        {
            string cacheKey = _cacheKeyPrefix + id;

            try
            {
                // Redis cache'ten veri almayı deneyin
                var cachedData = await _cache.StringGetAsync(cacheKey);
                if (!cachedData.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<T>(cachedData);
                }

                // Redis cache'te veri bulunamazsa, veritabanından veriyi çekin
                var data = await _context.Set<T>().FindAsync(id);

                // Veriyi Redis cache'ine ekleyin
                if (data != null)
                {
                    await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(data), TimeSpan.FromMinutes(5));
                }

                return data;
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Error retrieving data by ID: {ex.Message}");
                return null;
            }
        }

        public async Task AddAsync(T entity)
        {
            try
            {
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();

                // Cache'i temizle
                await InvalidateCacheAsync();
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Error adding data: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();

                // Cache'i temizle
                await InvalidateCacheAsync();
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Error updating data: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                var keyProperty = typeof(T).GetProperty("Id"); // Id özelliğini almak için
                var id = keyProperty?.GetValue(entity); // Id değerini almak için

                if (id != null)
                {
                    _context.Set<T>().Remove(entity);
                    await _context.SaveChangesAsync();

                    // Silinen veriyi Redis cache'inden temizleyin
                    await _cache.KeyDeleteAsync(_cacheKeyPrefix + id);
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Error deleting data: {ex.Message}");
                throw;
            }
        }

        private async Task InvalidateCacheAsync()
        {
            try
            {
                // Tüm cache'i temizle veya güncelle
                await _cache.KeyDeleteAsync(_cacheKeyPrefix + "All");
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Error invalidating cache: {ex.Message}");
            }
        }
    }
}
