using AutoMapper;
using CustomerService.DTOs.Responses.VehicleVersionDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;

namespace CustomerService.Infrastructure.Services;

public class VehicleVersionService
{
    private readonly VehicleVersionRepository _vehicleVersionRepository;
    private readonly IMapper _mapper;

    public VehicleVersionService(VehicleVersionRepository vehicleVersionRepository, IMapper mapper)
    {
        _vehicleVersionRepository = vehicleVersionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VersionBasicResponse>> GetVehicleVersionsByVehicleID(Guid vehicleId)
    {
        var versions = await _vehicleVersionRepository.GetVehicleVersionsByVehicleId(vehicleId);
        return _mapper.Map<IEnumerable<VersionBasicResponse>>(versions);
    }
}