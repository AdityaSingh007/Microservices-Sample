using Microservices.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microservices.Common.CustomHealthChecks
{
    public class IdentityServiceHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<IdentityServiceHealthCheck> logger;
        private readonly string IdentityServerHostname;

        public IdentityServiceHealthCheck(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<IdentityServiceHealthCheck> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.logger = logger;
            IdentityServerHostname = configuration["AuthenticationParameters:Hostname"] ?? throw new ArgumentNullException(nameof(configuration), "Authority configuration is missing.");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var defaultHttpClient = httpClientFactory.CreateClient(HttpClientConstants.DefaultHttpClientName);
                var healthCheckResponse = await defaultHttpClient.GetAsync(IdentityServerHostname + "/health");
                healthCheckResponse.EnsureSuccessStatusCode();
                var content = await healthCheckResponse.Content.ReadAsStringAsync();
                var checkResponse = JsonConvert.DeserializeObject<HealthCheckResponse>(content);
                if (checkResponse != null)
                {
                    var isHealthy = string.Equals(checkResponse.Status, "up", StringComparison.CurrentCultureIgnoreCase);
                    if (isHealthy)
                    {
                        return HealthCheckResult.Healthy("Identity service is healthy.");
                    }
                }

                return new HealthCheckResult(
                        context.Registration.FailureStatus, "Identity service is un-healthy.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return new HealthCheckResult(
                        context.Registration.FailureStatus, "Identity service is un-healthy.");
            }
        }
    }
}
