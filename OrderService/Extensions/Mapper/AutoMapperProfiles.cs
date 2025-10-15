using AutoMapper;
using IntelliAIService.DTOs.Requests;
using IntelliAIService.DTOs.Responses;
using OrderService.Entities;

namespace OrderService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<DateTime, DateOnly>().ConvertUsing(src => DateOnly.FromDateTime(src));
            CreateMap<DateTime?, DateOnly?>().ConvertUsing(src => src.HasValue ? DateOnly.FromDateTime(src.Value) : null);

            
            CreateMap<VehicleAllocation, VehicleAllocationResponse>()
                .ForMember(dest => dest.DealerName, 
                    opt => opt.MapFrom(src => src.Dealer.Name))
                .ForMember(dest => dest.VehicleName, 
                    opt => opt.MapFrom(src => 
                        $"{src.VehicleVersion.Vehicle.ModelName} ({src.VehicleVersion.VersionName})"));

            CreateMap<VehicleAllocationRequest, VehicleAllocation>();
        }
    }
}
