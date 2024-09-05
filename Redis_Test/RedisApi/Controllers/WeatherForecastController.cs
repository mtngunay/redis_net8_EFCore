using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisApi.Data.Entity;
using RedisApi.Data.Repository.Interface;
using RedisApi.Services;
using System.Diagnostics;
using System.Xml.Linq;

namespace RedisApi
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly CacheService _cacheService;
        private readonly IRepository<WeatherForecast> _repository;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(IRepository<WeatherForecast> repository, CacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();


            var cacheKey = "WeatherForecast:All";
            var cachedData = await _cacheService.GetAsync<List<WeatherForecast>>(cacheKey);

            if (cachedData == null)
            {
                // Cache'te veri yok, veritabanýndan alýn
                cachedData = await _repository.GetAllAsync();
                await _cacheService.SetAsync(cacheKey, cachedData, TimeSpan.FromMinutes(10));
            }

            sw.Stop();
            var s1 = sw.Elapsed.TotalMilliseconds;
            return Ok(cachedData);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var forecast = await _repository.GetByIdAsync(id);
            if (forecast == null)
            {
                return NotFound();
            }
            return Ok(forecast);
        }

        [HttpPost]
        public async Task<IActionResult> Add(WeatherForecast forecast)
        {
            await _repository.AddAsync(forecast);
            return CreatedAtAction(nameof(GetById), new { id = forecast.Id }, forecast);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WeatherForecast forecast)
        {
            if (id != forecast.Id)
            {
                return BadRequest();
            }

            await _repository.UpdateAsync(forecast);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var forecast = await _repository.GetByIdAsync(id);
            if (forecast == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(forecast);
            return NoContent();
        }
    }
}
