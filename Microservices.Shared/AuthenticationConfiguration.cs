using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Microservices.Shared
{
    public static class AuthenticationConfiguration
    {
        public static void ConfigureServicesAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authenticationOptions = GetAuthenticationOptions(configuration);

            services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.ConfigurationSection));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authenticationOptions.Authority;
                        options.RequireHttpsMetadata = authenticationOptions.RequireHttpsMetadata;
                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                        {
                            ValidateAudience = authenticationOptions.ValidateAudience,
                            ValidAudiences = authenticationOptions.Audience,
                            ValidateIssuer = true,
                            ValidateIssuerSigningKey = true,
                            ClockSkew = TimeSpan.FromSeconds(5)
                        };
                        options.Events = new JwtBearerEvents()
                        {
                            OnAuthenticationFailed = c =>
                            {
                                Console.WriteLine(c.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnForbidden = c =>
                            {
                                return Task.CompletedTask;
                            },

                        };
                    });
        }

        public static void ConfigureServicesAuthorization(this IServiceCollection services,
            IDictionary<string, Action<AuthorizationPolicyBuilder>> authorizationPolices
            )
        {
            services.AddAuthorization(options =>
            {
                foreach (var policy in authorizationPolices)
                {
                    options.AddPolicy(policy.Key, policy.Value);
                }
            });
        }

        public static void AddRoleClaimsTransformation(this IServiceCollection services)
        {
            services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();
        }

        private static AuthenticationOptions GetAuthenticationOptions(IConfiguration configuration)
        {
            var authenticationOptions = new AuthenticationOptions();
            configuration.GetSection(AuthenticationOptions.ConfigurationSection).Bind(authenticationOptions);

            if (string.IsNullOrWhiteSpace(authenticationOptions.Authority))
                throw new ArgumentNullException(nameof(authenticationOptions.Authority));

            if (!authenticationOptions.Audience.Any())
                throw new ArgumentException(nameof(authenticationOptions.Audience));

            return authenticationOptions;
        }
    }
}
