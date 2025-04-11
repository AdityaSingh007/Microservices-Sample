using Microservice3.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Microservice3.Infrastructure.Persistence
{
    public class AccountTransactionContext : DbContext
    {
        public AccountTransactionContext(DbContextOptions<AccountTransactionContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.Transaction> Transactions { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
