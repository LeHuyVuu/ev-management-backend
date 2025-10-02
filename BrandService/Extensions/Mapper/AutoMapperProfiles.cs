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

            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
            CreateMap<UserLoginRequest, User>();
            CreateMap<UserRegisterRequest, User>();
            CreateMap<UserUpdateRequest, User>();  
        }
    }
}
