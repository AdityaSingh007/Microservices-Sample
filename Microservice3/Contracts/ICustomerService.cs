using Microservice3.Models;

namespace Microservice3.Contracts
{
    public interface ICustomerService
    {
        Task<CustomerDto> GetCustomerDetails(Guid customerId);
    }
}
