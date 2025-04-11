using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microservice3.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microservice3.MappingProfile
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, GetAccountResponse>().ConvertUsing(src => new GetAccountResponse()
            {
                AccountId = src.Id.ToString(),
                CustomerId = src.CustomerId.ToString(),
                Balance = Convert.ToInt32(src.Balance),
                CreatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(src.CreatedDate, DateTimeKind.Utc)),
            });

            CreateMap<CreateAccountRequest, Account>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId));

            CreateMap<Account, CreateAccountResponse>().ConvertUsing(src => new CreateAccountResponse()
            {
                AccountId = src.Id.ToString(),
                CustomerId = src.CustomerId.ToString(),
                Balance = Convert.ToInt32(src.Balance),
                CreatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(src.CreatedDate, DateTimeKind.Utc)),
            });


            CreateMap<Account, AddResponse>()
               .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Balance));

            CreateMap<Account, WithdrawResponse>()
              .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Balance));
        }
    }
}
