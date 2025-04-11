using Microservice3.Domain.Entities;
using Microservice3.Infrastructure.Persistence;
using Microservice3.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Microservice3.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AccountTransactionContext _transactionContext;

        public TransactionRepository(AccountTransactionContext dbContext)
        {
            this._transactionContext = dbContext;
        }

        public async Task Add(Transaction transaction)
        {
            await _transactionContext.Transactions.AddAsync(transaction);
            await _transactionContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByAccountId(Guid accountId)
        {
            return await _transactionContext.Transactions.Where(x => x.AccountId == accountId).ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetWithFilter(Expression<Func<Transaction, bool>>? filter)
        {
            return await _transactionContext.Transactions.Where(filter).ToListAsync();
        }
    }
}
