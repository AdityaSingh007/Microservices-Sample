using Asp.Versioning;
using MassTransit;
using Microservice3.AuthorizatioHandlers;
using Microservice3.AuthorizationHandlers;
using Microservice3.EventBusConsumer;
using Microservice3.Extensions;
using Microservice3.Infrastructure.Persistence;
using Microservice3.Infrastructure.Repositories;
using Microservice3.Infrastructure.Repositories.Interface;
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

builder.Services.AddDbContext<AccountTransactionContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TransactionDb"), providerOptions => providerOptions.EnableRetryOnFailure
    (
        maxRetryCount: 10,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null
     ));
});

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Transaction.API", Version = "v1" });
});

builder.Services.ConfigureHealthChecks(builder.Configuration,
    databaseName: "transaction_db",
    connectionStringName: "TransactionDb");

builder.Services.AddHttpClient();

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


app.MigrateDatabase<AccountTransactionContext>((context, services) =>
{
    var logger = services.GetService<ILogger<AccountTransactionContextSeed>>();
    AccountTransactionContextSeed
        .SeedAsync(context, logger!)
        .Wait();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.RegisterServiceBusMessageEndpoints();
app.RegisterTransactionApiEndpoints();

app.MapServiceHealthChecks();

app.Run();
