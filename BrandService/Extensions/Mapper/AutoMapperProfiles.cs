using AutoMapper;
using BrandService.DTOs;
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
        }
    }
}
