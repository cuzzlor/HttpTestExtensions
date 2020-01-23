# HttpTestExtensions
Extensions to assist integration testing ASP.NET Core web applications.

## Integration testing multiple web applications using TestHost

> TestHost is an in-memory web host which doesn't support calls over the network

**Problem**: If you have web apps that call other web apps it can be confusing to set multiple  `TestHost`s which can call each other.

**Solution**: The `ReplaceHttpClient` extension method supports connecting `TestHost`s via dependency injection of `HttpClient`s.

*Note: this assumes an approach using `HttpClientFactory`s configured in DI.*

### Example

> Web app 1 is an API that returns data, web app 2 uses data from web app 1.

```cs
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
            await response.Content.ReadAsStreamAsync());
    }
}
```

You might set up the `WebApp1Client` in web app 2 (with endpoint, authorization, retry policy configuration etc):

```cs
services.AddHttpClient<IWebApp1Client, WebApp1Client>(client =>
                client.BaseAddress = new Uri(Configuration["Urls:WebApp1"])));
```

To make this work in integration tests, spinning up two TestHosts, you can supply `HttpClient`s from TestHost #1 to TestHost #2 in your `WebApplicationFactory.ConfigureWebHost` method.

```cs
builder.ConfigureServices(services =>
{
    services.ReplaceHttpClient<IWebApp1Client, WebApp1Client>(() => _webApplication1Factory.CreateClient());
});
```

A full example is available in the [tests](/tests) folder and below:

#### Integration Test

Using a `WebApplicationFactory` as the test fixture to provide access to the app under test.

```cs
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
```

#### Test Fixture

We use a custom `WebApplicationFactory<TStartup>` class to:
- Provide access to an web app #2 instance to test
- Provide web app #2 access to a web app #1 instance

```cs
public class WebApplicationFactory : WebApplicationFactory<WebApplication2.Startup>
{
    private readonly WebApplicationFactory<WebApplication1.Startup> _webApplication1Factory;

    public WebApplicationFactory()
    {
        _webApplication1Factory = new WebApplicationFactory<WebApplication1.Startup>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.ReplaceHttpClient<IWebApp1Client, WebApp1Client>(() => _webApplication1Factory.CreateClient());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _webApplication1Factory.Dispose();
    }
}
```






