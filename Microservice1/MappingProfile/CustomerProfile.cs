using AutoMapper;
using Microservice3.Domain.Entities;
using Microservice3.Models;

namespace Microservice3.MappingProfile
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerDto, Customer>();
        }
    }
}
