using AutoMapper;
using BrandService.DTOs;
using BrandService.DTOs.Requests.UserDTOs;
using BrandService.DTOs.Responses.UserDTOs;
using BrandService.Entities;

namespace BrandService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Dealer, DealerDto.DealerRequest>().ReverseMap();
            CreateMap<Dealer, DealerDto.DealerResponse>().ReverseMap();
            CreateMap<DealerTarget, DealerTargetDto>().ReverseMap();

            CreateMap<Promotion, PromotionDto.PromotionRequest>().ReverseMap();
            CreateMap<Promotion, PromotionDto.PromotionResponse>().ReverseMap();

            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
            CreateMap<User, UserLoginResponse>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));;
            CreateMap<UserLoginRequest, User>();
            CreateMap<UserRegisterRequest, User>();
            CreateMap<UserUpdateRequest, User>();  
        }
    }
}
