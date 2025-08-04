
using AutoMapper;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.Models.Finance;

namespace FinanceTracker.BusinessLogic.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionDto, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());
        
        CreateMap<Transaction, TransactionViewDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
    }
}