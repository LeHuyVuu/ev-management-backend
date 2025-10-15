using AutoMapper;
using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Requests.InventoryDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.DTOs.Responses.InventoryDTOs;
using BrandService.Entities;

namespace BrandService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // VEHICLE
            CreateMap<Vehicle, VehicleResponse>();
            CreateMap<Vehicle, VehicleDetailResponse>();
            CreateMap<VehicleRequest, Vehicle>();

            // VEHICLE VERSION
            CreateMap<VehicleVersionRequest, VehicleVersion>();
            CreateMap<VehicleVersion, DealerVehicleVersionResponse>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Vehicle.Brand))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Vehicle.ModelName))
            .ForMember(dest => dest.stockQuantity, opt => opt.MapFrom(src =>
                src.Inventories.Any() ? src.Inventories.FirstOrDefault().StockQuantity : 0
            ));
            CreateMap<VehicleVersion, BrandVehicleVersionResponse>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Vehicle.Brand))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Vehicle.ModelName))
            .ForMember(dest => dest.stockQuantity, opt => opt.MapFrom(src =>
                src.Inventories != null && src.Inventories.Any()
                    ? src.Inventories.Sum(i => i.StockQuantity) : 0
            ));

            // INVENTORY
            CreateMap<Inventory, InventoryResponse>();
            CreateMap<UpdateInventoryRequest, Inventory>();
        }
    }
}
