using Microservices.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microservices.Common.CorsConfiguration
{
    public static class CorsConfigurationSetup
    {
        public const string CorsPolicyName = "BFFCorsPolicy";
        public static void ConfigureApplicationCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var corsOptions = configuration.GetSection("Cors").Get<CorsOptions>();
            if (corsOptions == null || corsOptions.AllowedOrigins == null || !corsOptions.AllowedOrigins.Any())
            {
                throw new ArgumentException("Cors configuration is not properly set up. Please check your configuration.");
            }
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName,
                    policy =>
                {
                    if (!string.IsNullOrWhiteSpace(corsOptions.AllowedOrigins))
                    {
                        var allowedOrigins = corsOptions.AllowedOrigins.Split(",");
                        if (allowedOrigins.Length != 0)
                        {
                            var _ = allowedOrigins.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => policy.WithOrigins(x)).ToArray();
                        }
                        else
                            policy.WithOrigins(corsOptions.AllowedOrigins);

                    }
                    else
                        policy.AllowAnyOrigin();

                    if (!string.IsNullOrWhiteSpace(corsOptions.AllowedMethods))
                    {
                        var allowedMethods = corsOptions.AllowedMethods.Split(",");
                        var _ = allowedMethods.Select(x => policy.WithMethods(x)).ToArray();
                    }
                    else
                        policy.AllowAnyMethod();

                    if (!string.IsNullOrWhiteSpace(corsOptions.AllowedHeaders))
                    {
                        var allowedHeaders = corsOptions.AllowedHeaders.Split(",");
                        var _ = allowedHeaders.Select(x => policy.WithHeaders(x)).ToArray();
                    }
                    else
                        policy.AllowAnyHeader();

                    policy.AllowCredentials();

                });
            });
        }
    }
}
