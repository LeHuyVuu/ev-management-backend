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
            CreateMap<CreateVehicleRequest, Vehicle>();

            // VEHICLE VERSION
            CreateMap<VehicleVersion, VehicleVersionResponse>();
            CreateMap<CreateVehicleVersionRequest, VehicleVersion>();

            // INVENTORY
            CreateMap<Inventory, InventoryResponse>();
            CreateMap<UpdateInventoryRequest, Inventory>();
        }
    }
}
