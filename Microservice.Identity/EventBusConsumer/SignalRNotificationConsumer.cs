using Communcation.Contracts;
using MassTransit;
using Microservice.Identity.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Microservice.Identity.EventBusConsumer
{
    public class SignalRNotificationConsumer : IConsumer<SignalRNotificationEvent>
    {
        private readonly IHubContext<SignalRNotificationHub, ISignalRNotificationClient> hubContext;

        public SignalRNotificationConsumer(IHubContext<SignalRNotificationHub, ISignalRNotificationClient> hubContext)
        {
            this.hubContext = hubContext;
        }
        public async Task Consume(ConsumeContext<SignalRNotificationEvent> context)
        {
            await hubContext.Clients.All.SendNotification(context.Message.message, context.Message.payLoad);
        }
    }
}
