using AutoMapper;
using Microservice3.Domain.Entities;
using Microservice3.Interface;
using Microservice3.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Microservice3.EndpointHandlers
{
    public static class CustomerMessageHandler
    {
        public static async Task<Results<NotFound, Ok<CustomerDto>>> GetCustomerById(
            ICustomerRepository customerRepository,
            IMapper mapper,
            Guid customerId,
            ILogger<Customer> logger)
        {
            //todo:Pass user information through api gateway
            //logger.LogInformation("GetCustometById requested by - {@userIdentifier}", userIdentifier);

            var customer = await customerRepository.GetByIdAsync(customerId);

            if (customer == null)
            {
                logger.LogWarning("Customer with id:{@customerId} not found", customerId.ToString());
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<CustomerDto>(customer));
        }

        public static async Task<CreatedAtRoute<CustomerDto>> CreateCustomer(
            ICustomerRepository customerRepository,
            IMapper mapper,
            CustomerDto customerDto,
            ILogger<Customer> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            var customer = mapper.Map<Customer>(customerDto);
            customer = await customerRepository.AddAsync(customer);
            logger.LogInformation("Customer created with id:{@customerId}", customer.Id.ToString());
            httpContextAccessor.HttpContext?.Response?.Headers?.Append("customerId", customer.Id.ToString());
            return TypedResults.CreatedAtRoute(mapper.Map<CustomerDto>(customer), "GetCustomerById", new
            {
                customerId = customer.Id
            });
        }
    }
}
