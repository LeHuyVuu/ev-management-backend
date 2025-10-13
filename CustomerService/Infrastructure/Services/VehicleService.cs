using AutoMapper;
using CustomerService.Context;
using CustomerService.DTOs.Responses.VehicleDTOs;
using CustomerService.Infrastructure.Repositories;

namespace CustomerService.Infrastructure.Services;

public class VehicleService
{
    private readonly VehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public VehicleService(VehicleRepository vehicleRepository,  IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VehicleBasicResponse>> GetVehicles()
    {
        var vehicles = await _vehicleRepository.GetVehicles();
        return _mapper.Map<IEnumerable<VehicleBasicResponse>>(vehicles);
    }
}