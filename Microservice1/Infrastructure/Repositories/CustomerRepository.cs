using Microservice3.Infrastructure.Persistence;
using Microservice3.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Microservice3.Infrastructure.Repositories
{
    public class CustomerRepository : RepositoryBase<Domain.Entities.Customer>, ICustomerRepository
    {
        public CustomerRepository(CustomerContext dbContext) : base(dbContext)
        {

        }

        public async Task<bool> AnyAsync(Expression<Func<Domain.Entities.Customer, bool>> predicate)
        {
            return await _dbContext.Customers
                .AnyAsync(predicate);
        }
    }
}
