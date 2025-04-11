using Microservice3.AuthorizationHandlers;
using Microservices.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Microservice3.AuthorizatioHandlers
{
    public class MicroserviceScopeHandler : AuthorizationHandler<MicroserviceScopeRequirement>
    {
        private readonly IOptions<AuthenticationOptions> options;
        private readonly ILogger<MicroserviceScopeHandler> logger;

        public MicroserviceScopeHandler(IOptions<AuthenticationOptions> options , 
            ILogger<MicroserviceScopeHandler> logger)
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
                logger.LogError("Scope is not configured in the application settings.");
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
                logger.LogError($"Scope: {scopeClaim} not found");
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
