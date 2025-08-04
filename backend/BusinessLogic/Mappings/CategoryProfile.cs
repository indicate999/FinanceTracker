using AutoMapper;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.Models.Finance;

namespace FinanceTracker.BusinessLogic.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore());
        
        CreateMap<Category, CategoryViewDto>();
    }
}