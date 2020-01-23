using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace AspNetCore.Http.TestExtensions
{
    public static class HttpClientFactoryServiceCollectionTestExtensions
    {
        public static IServiceCollection ReplaceHttpClient<TClient, TImplementation>(this IServiceCollection services, Func<HttpClient> httpClientFunc) 
            where TImplementation : TClient 
            where TClient : class
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TClient));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddTransient<TClient>(s =>
            {
                var typedClientFactory = s.GetRequiredService<ITypedHttpClientFactory<TImplementation>>();
                return typedClientFactory.CreateClient(httpClientFunc());
            });

            return services;
        }

        public static IServiceCollection ReplaceHttpClient<TClient>(this IServiceCollection services, Func<HttpClient> httpClientFunc)
            where TClient : class
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TClient));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddTransient(s =>
            {
                var typedClientFactory = s.GetRequiredService<ITypedHttpClientFactory<TClient>>();
                return typedClientFactory.CreateClient(httpClientFunc());
            });

            return services;
        }
    }
}
