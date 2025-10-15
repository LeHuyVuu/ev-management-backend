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
            CreateMap<VehicleAllocation, VehicleAllocationResponse>();
            CreateMap<VehicleAllocationRequest, VehicleAllocation>();
        }
    }
}
