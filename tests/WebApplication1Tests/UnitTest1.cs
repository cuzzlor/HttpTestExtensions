using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1;
using Xunit;

namespace WebApplication1Tests
{
    public class UnitTest1 : IClassFixture<WebApplication1Factory>, IDisposable
    {
        private readonly WebApplication1Factory _factory;
        private readonly HttpClient _httpClient;

        public UnitTest1(WebApplication1Factory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task CanGetWeatherForecast()
        {
            var response = await _httpClient.GetAsync("weatherforecast");
            response.EnsureSuccessStatusCode();

            var forecasts = await JsonSerializer.DeserializeAsync<WeatherForecast[]>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions() {PropertyNameCaseInsensitive = true});

            Assert.NotEmpty(forecasts);
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}
