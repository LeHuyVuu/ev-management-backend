using AutoMapper;
using ProductService.DTOs;
using ProductService.Entities;

namespace ProductService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Customer, CustomerResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
        }
    }
}
