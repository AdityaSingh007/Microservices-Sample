using Communcation.Contracts;

namespace Microservice3.Infrastructure.Persistence
{
    public class AccountTransactionContextSeed
    {

        public static async Task SeedAsync(AccountTransactionContext transactionContext, ILogger<AccountTransactionContextSeed> logger)
        {
            if (!transactionContext.Transactions.Any())
            {
                transactionContext.Transactions.AddRange(GetPreconfiguredProducts());
                await transactionContext.SaveChangesAsync();
                logger.LogInformation($"Seed database associated with context {typeof(AccountTransactionContext).Name}");
            }
        }

        private static IEnumerable<Domain.Entities.Transaction> GetPreconfiguredProducts()
        {
            return new List<Domain.Entities.Transaction>()
            {
                new ()
                {
                    Id = Guid.NewGuid(),
                    AccountId = Guid.Parse("a3372135-ea3d-4eb9-8209-5a36634b2bba"),
                    Amount = 1_000_000,
                    CustomerId = Guid.Parse("ef533977-e666-4c75-ac4e-ea1de9ea4aef"),
                    Type = TransactionType.Adding,
                    CreatedDate = DateTime.UtcNow
                }
            };
        }
    }
}
