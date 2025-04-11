using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Microservices.Shared
{
    public class RoleClaimsTransformation : IClaimsTransformation
    {
        private readonly IOptions<AuthenticationOptions> options;

        public RoleClaimsTransformation(IOptions<AuthenticationOptions> options)
        {
            this.options = options;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;

            var resourceAccessClaim = identity?.FindFirst("resource_access");

            if (string.IsNullOrWhiteSpace(resourceAccessClaim?.Value))
            {
                throw new ArgumentNullException("resource_access claim found to be null");
            }

            foreach (var audience in options.Value.Audience)
            {
                var clientRoles = JObject.Parse(resourceAccessClaim.Value)
                                              .GetValue(audience)?
                                              .ToObject<JObject>()?
                                              .GetValue("roles")?
                                              .ToObject<List<string>>();

                if (clientRoles != null)
                {
                    foreach (var role in clientRoles)
                    {
                        // Add each role as a Claim of type ClaimTypes.Role
                        identity?.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            return Task.FromResult(principal);
        }
    }
}
