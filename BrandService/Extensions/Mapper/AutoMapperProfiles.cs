using AutoMapper;
using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Requests.InventoryDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.DTOs.Responses.InventoryDTOs;
using BrandService.Entities;
using BrandService.DTOs.Responses.DealerDTOs;

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

            CreateMap<VehicleVersion, BrandVehicleVersionResponse>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Vehicle.Brand))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Vehicle.ModelName))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src =>
                    src.BrandInventory != null ? src.BrandInventory.StockQuantity : 0))
                .ForMember(dest => dest.TotalStockQuantity, opt => opt.MapFrom(src =>
                    (src.BrandInventory != null ? src.BrandInventory.StockQuantity : 0)
                    + (src.Inventories != null ? src.Inventories.Sum(i => i.StockQuantity) : 0)));

            CreateMap<VehicleVersion, DealerVehicleVersionResponse>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Vehicle.Brand))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Vehicle.ModelName));

            // INVENTORY
            CreateMap<UpdateInventoryRequest, Inventory>();

            CreateMap<Inventory, InventoryResponse>()
                .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.InventoryId))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.Dealer, opt => opt.MapFrom(src => src.Dealer))
                .ForMember(dest => dest.VehicleVersion, opt => opt.MapFrom(src => src.VehicleVersion));

            // DEALER
            CreateMap<Dealer, DealerResponse>();
        }
    }
}
