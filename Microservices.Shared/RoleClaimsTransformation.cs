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
            var realmAccessClaim = identity?.FindFirst("realm_access");

            if (string.IsNullOrWhiteSpace(resourceAccessClaim?.Value) && string.IsNullOrWhiteSpace(realmAccessClaim?.Value))
            {
                throw new ArgumentNullException("role claim found to be null");
            }

            if (options.Value.Audience.Any())
            {
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
            }

            if (identity != null && !identity.HasClaim(x => x.Type.Equals(ClaimTypes.Role)))
            {
                var roles = JObject.Parse(realmAccessClaim.Value)
                                    .GetValue("roles")?
                                    .ToObject<List<string>>();
                if (roles != null)
                {
                    foreach (var role in roles)
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
