using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Microservices.Common
{
    public class AuthorizationConstants
    {
        public const string ValidateScopePolicyName = "ValidateScope";
        public const string ValidateAudiencesPolicyName = "ValidateAudiences";
        public const string BearerPolicyName = JwtBearerDefaults.AuthenticationScheme;
        public const string TokenResourceAccessClaim = "resource_access";
        public const string ValidateRolePolicyName = "callerShouldBeInRole";
    }
}
