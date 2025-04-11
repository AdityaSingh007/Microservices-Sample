using Microservices.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Microservice3.AuthorizationHandlers
{
    public class MicroserviceScopeHandler : AuthorizationHandler<MicroserviceScopeRequirement>
    {
        private readonly IOptions<AuthenticationOptions> options;
        private readonly ILogger<MicroserviceScopeHandler> logger;

        public MicroserviceScopeHandler(IOptions<AuthenticationOptions> options , ILogger<MicroserviceScopeHandler> logger)
        {
            this.options = options;
            this.logger = logger;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MicroserviceScopeRequirement requirement)
        {
            if (options == null || options.Value == null || string.IsNullOrEmpty(options.Value.Scope))
            {
                context.Fail();
                logger.LogError("Authorization options are not configured properly.");
                return Task.CompletedTask;
            }

            var claims = context.User.Identities.First().Claims;
            var scopeClaim = claims.FirstOrDefault(x => x.Type.Equals("scope", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
            if (scopeClaim.Contains(options.Value.Scope))
            {
                context.Succeed(requirement);
            }
            else
            {
                logger.LogError("Authorization failed. Required scope: {scope} not found.", options.Value.Scope);
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
