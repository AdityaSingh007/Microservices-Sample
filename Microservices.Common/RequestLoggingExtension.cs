using Microsoft.AspNetCore.Builder;
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
    }
}
