using Communcation.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Microservice3.EndpointHandlers
{
    public static class ServiceBusMessageHandler
    {
        public static async Task<Ok> PublishMessage(ServiceBusMessage serviceBusMessage,
           [FromServices] IPublishEndpoint publishEndpoint)
        {
            await publishEndpoint.Publish(serviceBusMessage);
            return TypedResults.Ok();
        }
    }
}
