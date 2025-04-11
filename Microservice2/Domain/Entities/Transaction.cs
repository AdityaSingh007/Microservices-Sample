using Communcation.Contracts;
using Microservice3.Domain.Common;

namespace Microservice3.Domain.Entities
{
    public class Transaction : EntityBase
    {
        public Guid AccountId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
    }
}
