using AutoMapper;
using FinancialService.Entities;
using FinancialService.Model;

namespace FinancialService.Mappings;

public class DealerDiscountProfile : Profile
{
    public DealerDiscountProfile()
    {
        // Entity -> DTO
        CreateMap<dealer_discount, DealerDiscountResponseDto>();
        CreateMap<dealer, DealerSummaryDto>();

        // DTO -> Entity
        CreateMap<DealerDiscountCreateDto, dealer_discount>()
            .ForMember(dest => dest.valid_from, opt => opt.MapFrom(src => DateOnly.Parse(src.valid_from)))
            .ForMember(dest => dest.valid_to, opt => opt.MapFrom(src => DateOnly.Parse(src.valid_to)));

        CreateMap<DealerDiscountUpdateDto, dealer_discount>()
            .ForMember(dest => dest.valid_from, opt => opt.MapFrom(src => DateOnly.Parse(src.valid_from)))
            .ForMember(dest => dest.valid_to, opt => opt.MapFrom(src => DateOnly.Parse(src.valid_to)));
    }
}