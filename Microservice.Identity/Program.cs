using Duende.Bff;
using Duende.Bff.Yarp;
using MassTransit;
using Microservice.Identity.EventBusConsumer;
using Microservice.Identity.Hubs;
using Microservice.Identity.Infrastructure;
using Microservices.Common;
using Microservices.Common.CorsConfiguration;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
        optional: false,
        reloadOnChange: true
     ).AddEnvironmentVariables();

builder.Host.UseSerilog(Logging.ConfigureLogger);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddMassTransit(x =>
{
    x.AddSqlMessageScheduler();

    x.UsingSqlServer((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumersFromNamespaceContaining<EventBusConsumerNamespace>();
});

var connectionString = builder.Configuration.GetConnectionString("MessagingDb");

builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        options.ConnectionString = connectionString;
    });

builder.Services.AddSqlServerMigrationHostedService();

builder.Services.AddBff()
    .AddRemoteApis();

builder.Services.AddReverseProxy()
                .AddBffExtensions()
                .ConfigureHttpClient((context, handler) =>
                {
                    handler.SslOptions.RemoteCertificateValidationCallback = (message, cert, chain, errors) => true;
                });

builder.Services.AddTransient<IReturnUrlValidator, FrontEndUrlValidator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
}).AddCookie("cookie", options =>
{
    options.Cookie.Name = "__Host-bff";
    options.Cookie.SameSite = SameSiteMode.Strict;
}).AddOpenIdConnect("oidc", options =>
{
    options.Authority = builder.Configuration["OIDC:Authority"];
    options.ClientId = builder.Configuration["OIDC:Client_Id"];
    options.ClientSecret = builder.Configuration["OIDC:Client_Secret"];
    options.ResponseType = "code";
    options.ResponseMode = "query";
    options.GetClaimsFromUserInfoEndpoint = true;
    options.MapInboundClaims = false;
    options.SaveTokens = true;
    options.UsePkce = true;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("offline_access");
    options.Scope.Add("ApiGateway_Fullaccess");
    options.SignedOutRedirectUri = builder.Configuration["OIDC:SignedOutRedirectUri"];
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
    options.Events = new OpenIdConnectEvents
    {
        OnTokenResponseReceived = context =>
         {
             var tokenResponse = context.TokenEndpointResponse;
             return Task.CompletedTask;
         },
        OnRedirectToIdentityProvider = context =>
         {
             // Ensure the redirect URI is set correctly for BFF
             //context.ProtocolMessage.RedirectUri = context.Request.Scheme + "://" + "localhost:4200" + "/bff/signin-oidc";
             Console.WriteLine($"Redirect URI: {context.ProtocolMessage.RedirectUri}");
             return Task.CompletedTask;
         },
    };
});

builder.Services.AddRoleClaimsTransformation();

builder.Services.AddAuthorization();

builder.Services.ConfigureHealthChecks(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.ConfigureApplicationCorsPolicy(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();
app.MapBffManagementEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors(CorsConfigurationSetup.CorsPolicyName);
app.AddMicroserviceRequestLogging();

// Comment this in to use the external api
app.MapRemoteBffApiEndpoint("/api", app.Configuration["RemoteBffApiEndpoint"])
   .RequireAccessToken(Duende.Bff.TokenType.User);

app.MapFallbackToFile("/index.html");

app.UseHttpsRedirection();
app.MapServiceHealthChecks();

app.MapHub<SignalRNotificationHub>("/notificationHub");

app.Run();