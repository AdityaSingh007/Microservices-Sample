using Asp.Versioning;
using MassTransit;
using Microservice3.AuthorizationHandlers;
using Microservice3.Extensions;
using Microservice3.Infrastructure.Persistence;
using Microservice3.Infrastructure.Repositories;
using Microservice3.Interface;
using Microservices.Common;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using ServiceBus.Consumers;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi 
builder.Host.UseSerilog(Logging.ConfigureLogger);

builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.AddSqlMessageScheduler();

    x.UsingSqlServer((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumersFromNamespaceContaining<ServiceBusConsumerNamespace>();
});

var connectionString = builder.Configuration.GetConnectionString("MessagingDb");

builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        options.ConnectionString = connectionString;
    });

builder.Services.AddSqlServerMigrationHostedService();

builder.Services.AddDbContext<CustomerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerConnectionString"), providerOptions => providerOptions.EnableRetryOnFailure
    (
        maxRetryCount: 10,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null
     ));
});

builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
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
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Customer.API", Version = "v1" });
});

builder.Services.ConfigureHealthChecks(builder.Configuration,
    databaseName: "customer_db",
    connectionStringName: "CustomerConnectionString");

builder.Services.AddSingleton<IAuthorizationHandler, MicroserviceScopeHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MicroserviceAudienceAuthorizationHandler>();

builder.Services.ConfigureServicesAuthentication(builder.Configuration);
builder.Services.ConfigureServicesAuthorization(new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
{
    { JwtBearerDefaults.AuthenticationScheme, policy => policy.RequireAuthenticatedUser() },
    { AuthorizationConstants.ValidateScopePolicyName, policy => policy.Requirements.Add(new MicroserviceScopeRequirement()) },
    { AuthorizationConstants.ValidateAudiencesPolicyName, policy => policy.Requirements.Add(new MicroserviceAudienceAuthorizationRequirement()) }
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MigrateDatabase<CustomerContext>((context, services) =>
{
    var logger = services.GetService<ILogger<CustomerContextSeed>>();
    CustomerContextSeed
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

app.RegisterServiceBusMessageEndpoints();
app.RegisterCustomerApiEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapServiceHealthChecks();

app.Run();