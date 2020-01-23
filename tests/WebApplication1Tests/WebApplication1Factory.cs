using AspNetCore.Http.TestExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApplication2;
using Startup = WebApplication1.Startup;

namespace WebApplication1Tests
{
    public class WebApplication1Factory : WebApplicationFactory<Startup>
    {
        // WebApplication1 calls WebApplication2
        private readonly WebApplicationFactory<WebApplication2.Startup> _webApplication2Factory;

        public WebApplication1Factory()
        {
            _webApplication2Factory = new WebApplicationFactory<WebApplication2.Startup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // replace the HttpClient for WebApp1Client one from WebApplicationFactory<WebApplication2>
                services.ReplaceHttpClient<IWebApp1Client, WebApp1Client>(() => _webApplication2Factory.CreateClient());
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _webApplication2Factory.Dispose();
        }
    }
}