using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Gateway.api.ApiGatewayTransform
{
    internal class TokenExchangeTransform : ITransformProvider
    {
        private readonly IOptions<Microservices.Shared.AuthenticationOptions> options;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDistributedCache cache;

        public TokenExchangeTransform(IOptions<Microservices.Shared.AuthenticationOptions> options,
            IHttpClientFactory httpClientFactory,
            IDistributedCache cache)
        {
            this.options = options;
            this.httpClientFactory = httpClientFactory;
            this.cache = cache;
        }
        public void Apply(TransformBuilderContext context)
        {
            context.AddRequestTransform(async (transformContext) =>
            {
                if (transformContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    var token = await transformContext.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");

                    var tokenFromCache = await cache.GetStringAsync(options.Value.TokenExchangeAudience);
                    if (!string.IsNullOrWhiteSpace(tokenFromCache))
                    {
                        transformContext.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenFromCache);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        using var client = httpClientFactory.CreateClient();
                        var discoveryDocument = await client.GetDiscoveryDocumentAsync(options.Value.Authority);

                        if (discoveryDocument.IsError)
                            throw new Exception(discoveryDocument.Error);

                        var tokenExchangeParams = new Parameters()
                    {
                            { "subject_token", token! },
                            { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" }
                    };

                        var tokenResponse = await client.RequestTokenExchangeTokenAsync(new TokenExchangeTokenRequest()
                        {
                            Address = discoveryDocument.TokenEndpoint,
                            ClientId = options.Value.ClientId ?? string.Empty,
                            ClientSecret = options.Value.ClientSecret ?? string.Empty,
                            Parameters = tokenExchangeParams,
                            GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                            Scope = options.Value.TokenExchangeScope ?? string.Empty,
                            Audience = options.Value.TokenExchangeAudience ?? string.Empty
                        });

                        if (tokenResponse.IsError)
                            throw new Exception(tokenResponse.Error);

                        await cache.SetStringAsync(options.Value.TokenExchangeAudience, tokenResponse.AccessToken, new DistributedCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn)
                        });

                        transformContext.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                    }
                }
            });
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {

        }

        public void ValidateRoute(TransformRouteValidationContext context)
        {

        }
    }
}
