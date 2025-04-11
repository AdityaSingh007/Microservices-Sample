using Microservices.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Microservice3.AuthorizatioHandlers
{
    public class MicroserviceAudienceAuthorizationHandler : AuthorizationHandler<MicroserviceAudienceAuthorizationRequirement>
    {
        private readonly IOptions<AuthenticationOptions> options;
        private readonly ILogger<MicroserviceAudienceAuthorizationHandler> logger;

        public MicroserviceAudienceAuthorizationHandler(IOptions<AuthenticationOptions> options, 
            ILogger<MicroserviceAudienceAuthorizationHandler> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MicroserviceAudienceAuthorizationRequirement requirement)
        {
            if (options == null || options.Value == null || string.IsNullOrEmpty(options.Value.Scope))
            {
                context.Fail();
                logger.LogError("Authentication options are not configured properly.");
                return Task.CompletedTask;
            }

            var claims = context.User.Identities.First().Claims;
            var audienceClaims = claims.Where(x => x.Type.Equals("aud", StringComparison.OrdinalIgnoreCase));
            var audienceIntersection = audienceClaims.Select(x => x.Value).Intersect(options.Value.Audience);
            if (!audienceIntersection.Any())
            {
                logger.LogError("No valid audience claims found in the token.");
                context.Fail();
            }
            else
            {
                var validAudienceIntersection = audienceIntersection.Intersect(options.Value.Audience).ToList();
                if (validAudienceIntersection.Count == options.Value.Audience.Count())
                {
                    context.Succeed(requirement);
                }
                else
                {
                    logger.LogError("Audience claims do not match the expected audience.");
                    context.Fail();
                }
            }

            return Task.CompletedTask;
        }
    }
}
