namespace Communcation.Contracts
{
    public class AccountTransactionEvent : IntegrationBaseEvent
    {
        public AccountTransactionEvent()
        {

        }

        public AccountTransactionEvent(Guid correlationId, DateTime createDate) : base(correlationId, createDate)
        {

        }
        public Guid AccountId { get; set; }
        public Guid CustomerId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
    }
}
