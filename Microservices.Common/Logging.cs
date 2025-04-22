using Elastic.Channels;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microservices.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace Microservices.Common
{
    public static class Logging
    {
        public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger =>
            (hostingContext, loggerConfiguration) =>
        {
            var env = hostingContext.HostingEnvironment;

            loggerConfiguration.MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", env.EnvironmentName)
            .Enrich.WithProperty("Application", env.ApplicationName)
            .Enrich.WithExceptionDetails()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console();

            if (hostingContext.HostingEnvironment.IsDevelopment())
            {
                loggerConfiguration.MinimumLevel.Override("Microservices", Serilog.Events.LogEventLevel.Warning);
            }

            var loggingOptions = new LoggingOptions();
            hostingContext.Configuration.GetSection(LoggingOptions.ConfigurationSection).Bind(loggingOptions);

            if (!string.IsNullOrEmpty(loggingOptions.LoggingUrl))
            {
                loggerConfiguration.WriteTo.Elasticsearch(new[] { new Uri(loggingOptions.LoggingUrl) }, opts =>
                {
                    opts.DataStream = new DataStreamName("logs", "microservicesdemo", "Microservices");
                    opts.TextFormatting = new EcsTextFormatterConfiguration();
                    opts.BootstrapMethod = BootstrapMethod.Failure;
                    opts.ConfigureChannel = channelOpts =>
                    {
                        channelOpts.BufferOptions = new BufferOptions { ExportMaxConcurrency = 10 };
                    };
                }, transport =>
                {
                    transport.ServerCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true);

                    transport.Authentication(new BasicAuthentication(loggingOptions.ElasticUserName, loggingOptions.ElasticPassword));
                });
            }
        };
    }
}
