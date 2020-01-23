using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWebApp1Client _webApp1Client;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWebApp1Client webApp1Client)
        {
            _webApp1Client = webApp1Client;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _webApp1Client.GetWeatherForecastsAsync();
        }
    }
}