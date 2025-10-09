using AutoMapper;
using IdentityService.DTOs.Requests.UserDTOs;
using IdentityService.DTOs.Responses.RoleDTOs;
using IdentityService.DTOs.Responses.UserDTOs;
using IdentityService.Entities;

namespace IdentityService.Extensions.Mapper
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
            CreateMap<Role, RoleResponse>();
        }
    }
}
