using Gateway.api.ApiGatewayTransform;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway.API", Version = "v1" });
});

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

builder.Services.AddHttpClient("KeycloakClient").ConfigurePrimaryHttpMessageHandler(() =>
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

builder.Services.ConfigureServicesAuthentication(builder.Configuration);
builder.Services.ConfigureServicesAuthorization(new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
{
    { JwtBearerDefaults.AuthenticationScheme, policy => policy.RequireAuthenticatedUser() },
});

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

app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");

app.Run();
