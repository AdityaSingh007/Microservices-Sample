using Gateway.api.ApiGatewayTransform;
using Microservices.Common.CustomHealthChecks;
using Microservices.Common.Http_Clients_Registration;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Configuration.AddJsonFile(
        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
        optional: false,
        reloadOnChange: true
     ).AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway.API", Version = "v1" });
});

builder.Services.AddHealthChecks()
                .AddCheck<IdentityServiceHealthCheck>("IdentityServiceHealth");

builder.Services.AddHealthChecksUI(setup =>
{
    setup.ConfigureApiEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestVersion = new Version(2, 0);
    });

    setup.UseApiEndpointHttpMessageHandler((sp) =>
    {
        return new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            }
        };
    });
})
.AddInMemoryStorage();

builder.Services.AddDistributedMemoryCache();

builder.Services.ConfigureServicesAuthentication(builder.Configuration);
builder.Services.ConfigureServicesAuthorization(new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
{
    { JwtBearerDefaults.AuthenticationScheme, policy => policy.RequireAuthenticatedUser() },
});

builder.Services.RegisterDefaultHttpClient(builder.Configuration);

builder.Services.AddRoleClaimsTransformation();

builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                .AddTransforms<TokenExchangeTransform>()
                .ConfigureHttpClient((context, handler) =>
                {
                   handler.SslOptions.RemoteCertificateValidationCallback = (message, cert, chain, errors) => true;
                });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapServiceHealthChecks();
app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");

app.Run();
