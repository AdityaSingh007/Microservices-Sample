using Asp.Versioning.Builder;
using Microservice3.EndpointHandlers;
using Microservices.Common;

namespace Microservice3.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void RegisterServiceBusMessageEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var serviceBusPublishMessageEndpoints = endpointRouteBuilder.MapGroup("/api/PublishMessage");
            serviceBusPublishMessageEndpoints.MapPost("", ServiceBusMessageHandler.PublishMessage);
        }

        public static void RegisterTransactionApiEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            ApiVersionSet apiVersionSet = endpointRouteBuilder.NewApiVersionSet()
                               .HasApiVersion(new Asp.Versioning.ApiVersion(1))
                               .ReportApiVersions()
                               .Build();

            var transactionApiEndpoints = endpointRouteBuilder.MapGroup("/api/v{apiVersion:apiVersion}/Transaction").WithApiVersionSet(apiVersionSet);
            transactionApiEndpoints.MapGet("/{accountId:guid}", TransactionMessageHandler.GetByAccountId).RequireAuthorization(
                AuthorizationConstants.BearerPolicyName,
                AuthorizationConstants.ValidateScopePolicyName,
                AuthorizationConstants.ValidateAudiencesPolicyName, 
                AuthorizationConstants.ValidateRolePolicyName);
        }
    }
}
