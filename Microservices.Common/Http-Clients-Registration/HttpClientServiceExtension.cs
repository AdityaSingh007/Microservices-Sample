using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microservices.Common.Http_Clients_Registration
{
    public static class HttpClientServiceExtension
    {
        public static void RegisterDefaultHttpClient(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpClient(HttpClientConstants.DefaultHttpClientName)
                    .AddPolicyHandler(HttpPolicy.GetRetryPolicy())
                    .AddPolicyHandler(HttpPolicy.GetCircuitBreakerPolicy())
                    .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
                };
            });
        }

        public static void AddTypedHttpClientWithPolicies<TService, TImplementation>(this IServiceCollection services,
            IConfiguration configuration)
        where TService : class
        where TImplementation : class, TService
        {
            services.AddHttpClient<TService, TImplementation>()
                    .AddPolicyHandler(HttpPolicy.GetRetryPolicy())
                    .AddPolicyHandler(HttpPolicy.GetCircuitBreakerPolicy())
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        var handler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                        };
                        return handler;
                    });
        }

        public static HttpClient CreateDefaultHttpClient(IConfiguration configuration)
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };
            return new HttpClient(handler);
        }

    }
}
