using AutoMapper;
using BrandService.DTOs.Requests.DealerDTOs;
using BrandService.DTOs.Requests.DealerTargetDTOs;
using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.DTOs.Responses.DealerTargetDTOs;
using BrandService.Entities;

namespace BrandService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Dealer, DealerRequest>().ReverseMap();
            CreateMap<Dealer, DealerResponse>().ReverseMap();

            CreateMap<DealerTargetRequest, DealerTarget>()
                .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToLower()))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.StartDate)));
            CreateMap<DealerTarget, DealerTargetResponse>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(dest => dest.Period, 
                    opt => opt.MapFrom(src => char.ToUpper(src.Period[0]) + src.Period.Substring(1).ToLower()
        ));
        }
    }
}
