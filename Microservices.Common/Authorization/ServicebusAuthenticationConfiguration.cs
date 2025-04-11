using Microservices.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microservices.Common.Authorization
{
    public static class ServicebusAuthenticationConfiguration
    {
        public static void ConfigureServiceBusAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServiceBusAuthenticationOption>(configuration.GetSection(ServiceBusAuthenticationOption.ConfigurationSection));
            services.AddSingleton<ICustomTokenValidation, CustomTokenValidationService>();
        }
    }
}
