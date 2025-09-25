using AutoMapper;
using BrandService.DTOs.Requests.DealerDTOs;
using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.Entities;

namespace BrandService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Dealer, DealerRequest>().ReverseMap();
            CreateMap<Dealer, DealerResponse>().ReverseMap();

        }
    }
}
