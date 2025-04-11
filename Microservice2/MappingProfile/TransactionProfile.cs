using AutoMapper;
using Communcation.Contracts;
using Microservice3.Domain.Entities;
using Microservice3.Models;

namespace Microservice3.MappingProfile
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, TransactionDto>();
            CreateMap<TransactionDto, Transaction>();
            CreateMap<AccountTransactionEvent, Transaction>();
        }
    }
}
