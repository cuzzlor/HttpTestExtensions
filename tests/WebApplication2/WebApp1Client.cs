using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApplication2
{
    public interface IWebApp1Client
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsAsync();
    }

    public class WebApp1Client : IWebApp1Client
    {
        private readonly HttpClient _httpClient;

        public WebApp1Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastsAsync()
        {
            var response = await _httpClient.GetAsync("weatherforecast");
            response.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<WeatherForecast[]>(
                await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
