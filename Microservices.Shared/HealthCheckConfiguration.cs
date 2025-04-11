using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microservices.Shared
{
    public static class HealthCheckConfiguration
    {
        public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, string databaseName = "", string connectionStringName = "")
        {
            string connectionString = configuration[$"ConnectionStrings:{connectionStringName}"] ?? string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string {connectionStringName} is not found in configuration");
            }

            services.AddHealthChecks()
                    .AddSqlServer(connectionString, healthQuery: "select 1", name: "SQL Server", 
                    failureStatus: HealthStatus.Unhealthy, tags: new[] { "Service", databaseName });
        }
    }
}
