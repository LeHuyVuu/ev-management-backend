using AutoMapper;
using BrandService.DTOs;
using BrandService.Entities;

namespace BrandService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Dealer, DealerDTO.DealerRequest>().ReverseMap();
            CreateMap<Dealer, DealerDTO.DealerResponse>().ReverseMap();
            CreateMap<DealerTarget, DealerTargetDTO>().ReverseMap();

            CreateMap<Promotion, PromotionDTO.PromotionRequest>().ReverseMap();
            CreateMap<Promotion, PromotionDTO.PromotionResponse>().ReverseMap();
        }
    }
}
