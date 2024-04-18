using Microsoft.EntityFrameworkCore;
using RedisApi.Data.Repository.Interface;
using StackExchange.Redis;

namespace RedisApi.Data.Repository.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context; 
        private readonly ConnectionMultiplexer _redis;

        public Repository(AppDbContext context, ConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var entity = _context.Set<T>();
            string tableName = entity.EntityType.ConstructorBinding.RuntimeType.Name;
            string key = $"{tableName}";

            #region REDIS

            //Bunun için docker desktop kurmak gerekmektedir.
            //var db = _redis.GetDatabase();

            //var getRedis = await db.StringGetAsync(key);

            //if (getRedis.IsNull)
            //{
            //    db.StringSetAsync(key, value);
            //} 

            //public async Task<string> GetValueAsync(string key)
            //{
            //    var db = _redis.GetDatabase();
            //    return await db.StringGetAsync(key);
            //}

            //public async Task SetValueAsync(string key, string value)
            //{
            //    var db = _redis.GetDatabase();
            //    await db.StringSetAsync(key, value);
            //}

            //public async Task<bool> DeleteKeyAsync(string key)
            //{
            //    var db = _redis.GetDatabase();
            //    return await db.KeyDeleteAsync(key);
            //}
            #endregion


            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
