using AspNetCore.Http.TestExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApplication2;

namespace WebApplication2Tests
{
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
}