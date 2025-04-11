using Microservice3.AuthorizationHandlers;
using Microservices.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Microservice3.AuthorizatioHandlers
{
    public class MicroserviceScopeHandler : AuthorizationHandler<MicroserviceScopeRequirement>
    {
        private readonly IOptions<AuthenticationOptions> options;

        public MicroserviceScopeHandler(IOptions<AuthenticationOptions> options)
        {
            this.options = options;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MicroserviceScopeRequirement requirement)
        {
            if (options == null || options.Value == null || string.IsNullOrEmpty(options.Value.Scope))
            {
                context.Fail();
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
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
