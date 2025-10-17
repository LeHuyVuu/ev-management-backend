using AutoMapper;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Entities;

namespace OrderService.Extensions.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<DateTime, DateOnly>().ConvertUsing(src => DateOnly.FromDateTime(src));
            CreateMap<DateTime?, DateOnly?>().ConvertUsing(src => src.HasValue ? DateOnly.FromDateTime(src.Value) : null);

            //  VehicleAllocation 
            CreateMap<VehicleAllocation, VehicleAllocationResponse>()
                .ForMember(dest => dest.DealerName, 
                    opt => opt.MapFrom(src => src.Dealer.Name))
                .ForMember(dest => dest.VehicleName, 
                    opt => opt.MapFrom(src => 
                        $"{src.VehicleVersion.Vehicle.ModelName} ({src.VehicleVersion.VersionName})"));

            CreateMap<VehicleAllocationRequest, VehicleAllocation>();
            
            //  VehicleTransferOrder
            CreateMap<VehicleTransferOrder, VehicleTransferOrderResponse>()
                .ForMember(dest => dest.FromDealerName,
                    opt => opt.MapFrom(src => src.FromDealer != null ? src.FromDealer.Name : string.Empty))
                .ForMember(dest => dest.ToDealerName,
                    opt => opt.MapFrom(src => src.ToDealer != null ? src.ToDealer.Name : string.Empty))
                .ForMember(dest => dest.VehicleName,
                    opt => opt.MapFrom(src =>
                        src.VehicleVersion != null
                            ? $"{src.VehicleVersion.Vehicle.ModelName} ({src.VehicleVersion.VersionName})"
                            : string.Empty));

            CreateMap<VehicleTransferOrderRequest, VehicleTransferOrder>();
            
            
            CreateMap<TestDrife, TestDriveResponse>()
                .ForMember(dest => dest.DealerName, otp => otp.MapFrom(src => src.Dealer.Name))
                .ForMember(dest => dest.CustomerName, otp => otp.MapFrom(src => src.Customer.Name))
                .ForMember(dest => dest.StaffUserName, otp => otp.MapFrom(src => src.StaffUser.Name))
                .ForMember(dest => dest.VehicleName,
                    opt => opt.MapFrom(src =>
                        src.VehicleVersion != null
                            ? $"{src.VehicleVersion.Vehicle.ModelName} ({src.VehicleVersion.VersionName})"
                            : string.Empty));
            CreateMap<TestDriveRequest, TestDrife>();
        }
    }
}
