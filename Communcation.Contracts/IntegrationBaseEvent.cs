namespace Communcation.Contracts
{
    public class IntegrationBaseEvent
    {
        public IntegrationBaseEvent()
        {
            CorrelationId = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public IntegrationBaseEvent(Guid id, DateTime createDate)
        {
            CorrelationId = id;
            CreationDate = createDate;
        }

        public Guid CorrelationId { get; set; }

        public DateTime CreationDate { get; private set; }

        public SecurityContext? SecurityContext { get; set; }
    }
}
