using System.Linq.Expressions;

namespace Microservice3.Interface
{
    public interface ICustomerRepository : IRepositoryBase<Domain.Entities.Customer>
    {
        Task<bool> AnyAsync(Expression<Func<Domain.Entities.Customer, bool>> predicate);
    }
}
