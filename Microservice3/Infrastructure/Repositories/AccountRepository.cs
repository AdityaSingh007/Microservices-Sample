using Microservice3.Infrastructure.Persistence;

namespace Microservice3.Infrastructure.Repositories
{
    public class AccountRepository : RepositoryBase<Domain.Entities.Account>, IAccountRepository
    {
        public AccountRepository(AccountContext dbContext) : base(dbContext)
        {
        }
    }
}
