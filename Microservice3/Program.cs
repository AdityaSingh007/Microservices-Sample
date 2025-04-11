using MassTransit;
using Microservice3.AuthorizatioHandlers;
using Microservice3.AuthorizationHandler;
using Microservice3.AuthorizationHandlers;
using Microservice3.Contracts;
using Microservice3.Extensions;
using Microservice3.Infrastructure.Persistence;
using Microservice3.Infrastructure.Policy;
using Microservice3.Infrastructure.Repositories;
using Microservice3.Services;
using Microservices.Common;
using Microservices.Common.Authorization;
using Microservices.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Logging.ConfigureLogger);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddGrpc().AddJsonTranscoding(o =>
{
    o.JsonSettings.WriteIndented = true;
});

builder.Services.AddGrpcSwagger();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "Customer account api", Version = "v1" });
});

builder.Services.AddMassTransit(x =>
{
    x.AddSqlMessageScheduler();

    x.UsingSqlServer((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });
});

var connectionString = builder.Configuration.GetConnectionString("MessagingDb");

builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        options.ConnectionString = connectionString;
    });

builder.Services.AddSqlServerMigrationHostedService();

builder.Services.AddDbContext<AccountContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AccountDb"), providerOptions => providerOptions.EnableRetryOnFailure
    (
        maxRetryCount: 10,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null
     ));
});

builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddHttpClient<ICustomerService, CustomerService>()
                .AddPolicyHandler(HttpPolicy.GetRetryPolicy())
                .AddPolicyHandler(HttpPolicy.GetCircuitBreakerPolicy());

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.ConfigureHealthChecks(builder.Configuration,
    databaseName: "account_db",
    connectionStringName: "AccountDb");

builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpContextAccessor();

builder.Services.AddClientCredentialsTokenManagement()
    .AddClient(builder.Configuration["ServiceBusAuthenticationParameters:ServiceBus_ClientId"] ?? string.Empty, client =>
    {
        client.TokenEndpoint = builder.Configuration["ServiceBusAuthenticationParameters:TokenEndpoint"];
        client.ClientId = builder.Configuration["ServiceBusAuthenticationParameters:ServiceBus_ClientId"];
        client.ClientSecret = builder.Configuration["ServiceBusAuthenticationParameters:ServiceBus_ClientSecret"];
        client.Scope = builder.Configuration["ServiceBusAuthenticationParameters:ServiceBus_Scope"];
    });

builder.Services.AddSingleton<IAuthorizationHandler, MicroserviceScopeHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MicroserviceAudienceAuthorizationHandler>();

builder.Services.ConfigureServicesAuthentication(builder.Configuration);

builder.Services.ConfigureServicesAuthorization(new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
{
    { AuthorizationConstants.BearerPolicyName, policy => policy.RequireAuthenticatedUser() },
    { AuthorizationConstants.ValidateScopePolicyName, policy => policy.Requirements.Add(new MicroserviceScopeRequirement()) },
    { AuthorizationConstants.ValidateAudiencesPolicyName, policy => policy.Requirements.Add(new MicroserviceAudienceAuthorizationRequirement()) },
    { AuthorizationConstants.ValidateRolePolicyName, policy => policy.RequireRole("transaction_manager")}
});

builder.Services.ConfigureServiceBusAuthentication(builder.Configuration);

builder.Services.AddRoleClaimsTransformation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MigrateDatabase<AccountContext>((context, services) =>
{
    var logger = services.GetService<ILogger<AccountContextSeed>>();
    AccountContextSeed
        .SeedAsync(context, logger!)
        .Wait();
});

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<CustomerAccountService>();

app.MapServiceHealthChecks();

app.Run();
