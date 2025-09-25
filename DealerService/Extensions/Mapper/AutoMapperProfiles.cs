using AutoMapper;
using ProductService.DTOs;
using ProductService.Entities;

namespace ProductService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Dealer, DealerDto.DealerRequest>().ReverseMap();
            CreateMap<Dealer, DealerDto.DealerResponse>().ReverseMap();
            CreateMap<DealerTarget, DealerTargetDto>().ReverseMap();
        }
    }
}
