using Communcation.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ServiceBus.Consumers
{
    public class ServiceBusMessageConsumer : IConsumer<ServiceBusMessage>
    {
        readonly ILogger<ServiceBusMessageConsumer> _logger;

        public ServiceBusMessageConsumer(ILogger<ServiceBusMessageConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<ServiceBusMessage> context)
        {
            var logMessage = $"Message received on service bus with id - {context.Message.Id} placed by - {context.Message.CreatedBy}";
            _logger.LogInformation(logMessage);
            _logger.LogInformation(context.Message.Message);
        }
    }
}
