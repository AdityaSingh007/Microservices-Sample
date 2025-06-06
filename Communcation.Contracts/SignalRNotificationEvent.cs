namespace Communcation.Contracts
{
    public class SignalRNotificationEvent
    {
        public Guid messageId { get; set; } = Guid.NewGuid();
        public string message { get; set; } = string.Empty;
        public dynamic payLoad { get; set; } = default!;
    }
}
