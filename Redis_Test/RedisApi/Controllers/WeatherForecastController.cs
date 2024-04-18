using Microsoft.AspNetCore.Mvc;
using RedisApi.Data.Entity;
using RedisApi.Data.Repository.Interface;

namespace RedisApi
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IRepository<WeatherForecast> _repository;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(IRepository<WeatherForecast> repository)
        {
            _repository = repository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var forecasts = await _repository.GetAllAsync();
            return Ok(forecasts);
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
