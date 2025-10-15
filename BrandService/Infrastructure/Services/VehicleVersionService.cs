using AutoMapper;
using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;
using BrandService.Models;

namespace BrandService.Infrastructure.Services
{
    public class VehicleVersionService
    {
        private readonly VehicleVersionRepository _repo;
        private readonly IMapper _mapper;
        public VehicleVersionService(VehicleVersionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BrandVehicleVersionResponse>> AddVersionAsync(Guid vehicleId, VehicleVersionRequest request)
        {
            var version = _mapper.Map<VehicleVersion>(request);
            version.VehicleId = vehicleId;
            var added = await _repo.AddVersionAsync(version);
            return ApiResponse<BrandVehicleVersionResponse>.Success(_mapper.Map<BrandVehicleVersionResponse>(added), "Version created");
        }

        public async Task<ApiResponse<BrandVehicleVersionResponse>> UpdateVersionAsync(Guid vehicleVersionId, VehicleVersionRequest request)
        {
            var version = _mapper.Map<VehicleVersion>(request);
            version.VehicleVersionId = vehicleVersionId;
            var updated = await _repo.UpdateVersionAsync(version);
            return ApiResponse<BrandVehicleVersionResponse>.Success(_mapper.Map<BrandVehicleVersionResponse>(updated), "Version updated");
        }

        public async Task<ApiResponse<BrandVehicleVersionResponse>> GetVersionByIdAsync(Guid id)
        {
            var version = await _repo.GetVersionByIdAsync(id);
            return ApiResponse<BrandVehicleVersionResponse>.Success(_mapper.Map<BrandVehicleVersionResponse>(version));
        }
    }
}
