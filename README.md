# HttpTestExtensions
Extensions to assist integration testing ASP.NET Core web applications.

## Installation
```
Install-Package HttpTestExtensions
```

## Usage
```cs
public class WebApplicationFactory : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.ReplaceHttpClient<ISomeAppClient, SomeAppClient>(() => _someAppFactory.CreateClient());
```

## Long read: integration testing multiple web applications using TestHost

> TestHost is an in-memory web host which doesn't support calls over the network

**Problem**:
If you a web app that calls another web app in your solution, it can be tricky to set up multiple `TestHost`s which can call each other.

**Solution**:
The `ReplaceHttpClient` extension method supports connecting `TestHost`s by replacing the `HttpClient` they are injected with.

*Note: this assumes an [approach using Typed clients `IHttpClientFactory` configuration in DI](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1#typed-clients).*


**Disclaimer**:
Obviously having the complexity of one app depending on another isn't necessarily ideal and I'm not suggesting this is a pattern to emulate. However I have had the need to call into a TestHost enough times to want to be able to set this up via DI.

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

`WebApp1Client` is set up in `ConfigureServices`:

```cs
services.AddHttpClient<IWebApp1Client, WebApp1Client>(client =>
                client.BaseAddress = new Uri(Configuration["Urls:WebApp1"])));
```

To make this work in an integration test that spins up two TestHosts, you can supply an `HttpClient` from one TestHost to consuming services in the other in the TestHost `ConfigureServices` method:

```cs
builder.ConfigureServices(services =>
{
    services.ReplaceHttpClient<IWebApp1Client, WebApp1Client>(() => _webApplication1Factory.CreateClient());
});
```

A working example is available in the [tests](/tests) folder. For more context read the code samples below:

#### Integration Test

A `WebApplicationFactory` test fixture provides access to the app under test - completely normal.

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
- Provide access to an instance of the app under test (WebApplication2)
- Provide access to an instance of the other app required (WebApplication1) 

```cs
public class WebApplicationFactory : WebApplicationFactory<WebApplication2.Startup>
{
    // we need to call WebApplication1
    private readonly WebApplicationFactory<WebApplication1.Startup> _webApplication1Factory;

    public WebApplicationFactory()
    {
        _webApplication1Factory = new WebApplicationFactory<WebApplication1.Startup>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // provide an HttpClient from WebApplicationFactory<WebApplication1> to WebApp1Client
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

If you have the need to hook up TestHosts together, hopefully this extension makes it cleaner.
