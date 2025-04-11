using System.Linq.Expressions;

namespace Microservice3.Infrastructure.Repositories.Interface
{
    public interface ITransactionRepository
    {
        Task Add(Domain.Entities.Transaction transaction);
        Task<IEnumerable<Domain.Entities.Transaction>> GetByAccountId(Guid accountId);
        Task<IEnumerable<Domain.Entities.Transaction>> GetWithFilter(Expression<Func<Domain.Entities.Transaction, bool>>? filter);
    }
}
