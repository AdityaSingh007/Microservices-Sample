namespace Communcation.Contracts
{
    public class ServiceBusMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
