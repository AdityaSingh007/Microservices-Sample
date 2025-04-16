using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microservices.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Microservices.Common.Authorization
{
    public class CustomTokenValidationService : ICustomTokenValidation
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOptions<ServiceBusAuthenticationOption> options;
        private readonly ILogger<CustomTokenValidationService> logger;

        public CustomTokenValidationService(IHttpClientFactory httpClientFactory,
            IOptions<ServiceBusAuthenticationOption> options,
            ILogger<CustomTokenValidationService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.options = options;
            this.logger = logger;
        }
        public async Task<bool> ValidateToken(string token,
            TokenValidationParameters tokenValidationParameters)
        {
            try
            {
                using var _client = httpClientFactory.CreateClient("KeycloakClient");
                var discoveryDocumentResponse = await _client.GetDiscoveryDocumentAsync(options.Value.Authority);
                if (discoveryDocumentResponse.IsError)
                {
                    logger.LogError("Error while getting discovery document: {error}", discoveryDocumentResponse.Error);
                    throw new Exception(discoveryDocumentResponse.Error);
                }
                var issuerSigningKeys = new List<SecurityKey>();
                if (discoveryDocumentResponse?.KeySet?.Keys != null)
                {
                    foreach (var key in discoveryDocumentResponse.KeySet.Keys)
                    {
                        var e = Base64Url.Decode(key.E);
                        var n = Base64Url.Decode(key.N);
                        var rsa = new RsaSecurityKey(new RSAParameters
                        {
                            Exponent = e,
                            Modulus = n
                        });
                        rsa.KeyId = key.Kid;

                        issuerSigningKeys.Add(rsa);
                    }
                }
                else
                {
                    logger.LogError("KeySet or Keys is null in discovery document response.");
                    throw new Exception("KeySet or Keys is null in discovery document response.");
                }

                tokenValidationParameters.IssuerSigningKeys = issuerSigningKeys;
                tokenValidationParameters.ValidateIssuerSigningKey = true;

                var tokenValidationResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(token, tokenValidationParameters);

                if (!tokenValidationResult.IsValid)
                    logger.LogError("Token is not valid: {error}", tokenValidationResult.Exception.Message);

                return tokenValidationResult.IsValid;
            }
            catch (Exception ex)
            {
                logger.LogError("Error while validating token: {error}", ex.Message);
                return false;
            }
        }
    }
}
