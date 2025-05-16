using Duende.Bff.Yarp;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
        optional: false,
        reloadOnChange: true
     ).AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddBff()
    .AddRemoteApis();

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
    options.Events = new OpenIdConnectEvents
    {
        OnTokenResponseReceived = context =>
         {
             var tokenResponse = context.TokenEndpointResponse;
             return Task.CompletedTask;
         },
    };
});

builder.Services.AddRoleClaimsTransformation();

builder.Services.AddAuthorization();

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

// Comment this in to use the external api
app.MapRemoteBffApiEndpoint("/api", app.Configuration["RemoteBffApiEndpoint"])
   .RequireAccessToken(Duende.Bff.TokenType.User);

app.MapFallbackToFile("/index.html");

app.UseHttpsRedirection();
app.Run();