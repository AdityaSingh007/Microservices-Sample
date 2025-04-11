using Asp.Versioning.Builder;
using Microservice3.EndpointHandlers;
using Microservices.Common;

namespace Microservice3.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void RegisterServiceBusMessageEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            ApiVersionSet apiVersionSet = endpointRouteBuilder.NewApiVersionSet()
                                .HasApiVersion(new Asp.Versioning.ApiVersion(1))
                                .ReportApiVersions()
                                .Build();

            var serviceBusPublishMessageEndpoints = endpointRouteBuilder.MapGroup("/api/v{apiVersion:apiVersion}/PublishMessage").WithApiVersionSet(apiVersionSet);
            serviceBusPublishMessageEndpoints.MapPost("", ServiceBusMessageHandler.PublishMessage);
        }

        public static void RegisterCustomerApiEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            ApiVersionSet apiVersionSet = endpointRouteBuilder.NewApiVersionSet()
                                .HasApiVersion(new Asp.Versioning.ApiVersion(1))
                                .ReportApiVersions()
                                .Build();

            var customerApiEndpoints = endpointRouteBuilder.MapGroup("/api/v{apiVersion:apiVersion}/Customer").WithApiVersionSet(apiVersionSet);
            customerApiEndpoints.MapGet("/{customerId:guid}", CustomerMessageHandler.GetCustometById).WithName("GetCustometById").RequireAuthorization(
                AuthorizationConstants.BearerPolicyName,
                AuthorizationConstants.ValidateScopePolicyName,
                AuthorizationConstants.ValidateAudiencesPolicyName);

            customerApiEndpoints.MapPost("", CustomerMessageHandler.CreateCustomer).RequireAuthorization(
                AuthorizationConstants.BearerPolicyName,
                AuthorizationConstants.ValidateScopePolicyName,
                AuthorizationConstants.ValidateAudiencesPolicyName);
        }
    }
}
