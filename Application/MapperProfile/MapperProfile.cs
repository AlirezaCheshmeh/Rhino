using Application.Mediator.Banks.DTOs;
using Application.Mediator.Reminders.DTOs;
using Application.Mediator.Transactions.DTOs;
using Application.Services.TelegramServices.BaseMethods;
using AutoMapper;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Domain.Entities.Transactions;

namespace Application.MapperProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Category, NameValueDTO>().ReverseMap();
            CreateMap<Transaction, TransactionDTO>().ReverseMap();
            CreateMap<TransactionDto, TransactionDTO>().ReverseMap();
            CreateMap<ReminderDto,ReminderDTO>().ReverseMap();
            CreateMap<BankDTO, Bank>().ReverseMap();
        }
    }

    //DTO
    public class NameValueDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
