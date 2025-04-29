using AutoMapper;
using InternetCafe.Application.DTOs.Account;
using InternetCafe.Application.DTOs.Computer;
using InternetCafe.Application.DTOs.Session;
using InternetCafe.Application.DTOs.Transaction;
using InternetCafe.Application.DTOs.User;
using InternetCafe.Domain.Entities;
using System;

namespace InternetCafe.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.Creation_Timestamp));

            CreateMap<User, UserDetailsDTO>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.Creation_Timestamp));

            CreateMap<Computer, ComputerDTO>();
            CreateMap<Computer, ComputerDetailsDTO>();

            CreateMap<Account, AccountDTO>();
            CreateMap<Account, AccountDetailsDTO>();

            CreateMap<Session, SessionDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<Session, SessionDetailsDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<Session, SessionSummaryDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<Transaction, TransactionDTO>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.HasValue ? (int)src.PaymentMethod.Value : (int?)null))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Creation_Timestamp));
        }
    }
}