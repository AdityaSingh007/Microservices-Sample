using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microservices.Common.CustomHealthChecks
{
    public class IdentityServiceHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("Identity service is healthy."));
            }

            return Task.FromResult(
                new HealthCheckResult(
                    context.Registration.FailureStatus, "Identity service is un-healthy."));
        }
    }
}
