using Microservices.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.AspNetCore;

namespace Microservices.Common
{
    public static class RequestLoggingExtension
    {
        public static void AddMicroserviceRequestLogging(this IApplicationBuilder app, Action<RequestLoggingOptions>? configureOptions = null)
        {
            if (configureOptions != null)
                app.UseSerilogRequestLogging(configureOptions);
            else
                app.UseSerilogRequestLogging(opts =>
                {
                    opts.GetLevel = (ctx, elapsed, ex) => LogHelper.ExcludeHealthChecks(ctx, elapsed, ex!);
                });
        }

        public static void RegisterLoggingParameterOptions(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.ConfigurationSection));
        }
    }
}
