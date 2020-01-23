using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1;
using Xunit;

namespace WebApplication2Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _sut;

        public IntegrationTests(WebApplicationFactory sut)
        {
            _sut = sut;
        }

        [Fact]
        public async Task CanGetWeatherForecast()
        {
            var httpClient = _sut.CreateClient();

            var response = await httpClient.GetAsync("weatherforecast");
            response.EnsureSuccessStatusCode();

            var forecasts = await JsonSerializer.DeserializeAsync<WeatherForecast[]>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions() {PropertyNameCaseInsensitive = true});

            Assert.NotEmpty(forecasts);
        }
    }
}
