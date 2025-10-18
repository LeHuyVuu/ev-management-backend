using AutoMapper;
using DealerService.DTOs.Requests.DealerDTOs;
using DealerService.DTOs.Requests.DealerTargetDTOs;
using DealerService.DTOs.Responses.DealerDTOs;
using DealerService.DTOs.Responses.DealerTargetDTOs;
using DealerService.Entities;

namespace DealerService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Dealer
            CreateMap<DealerRequest, Dealer>();
            CreateMap<Dealer, DealerResponse>();

            // Dealer Target
            CreateMap<DealerTargetRequest, DealerTarget>()
                .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToLower()))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.StartDate)));
            CreateMap<DealerTarget, DealerTargetResponse>()
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => src.StartDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(dest => dest.Period,
                    opt => opt.MapFrom(src => char.ToUpper(src.Period[0]) + src.Period.Substring(1).ToLower()))
                .ForMember(dest => dest.DealerName, opt => opt.MapFrom(src => src.Dealer.Name));

            // User
            CreateMap<User, DealerStaffsResponse>()
                .ForAllMembers(opt
                    => opt.Condition((src, dest, srcMember)
                        => srcMember != null));
        }
    }
}
