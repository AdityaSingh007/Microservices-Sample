using Microsoft.IdentityModel.Tokens;

namespace Microservices.Common.Authorization
{
    public interface ICustomTokenValidation
    {
        Task<bool> ValidateToken(string token , TokenValidationParameters tokenValidationParameters);
    }
}
