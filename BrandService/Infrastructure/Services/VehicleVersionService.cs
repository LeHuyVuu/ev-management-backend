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

        public async Task<ApiResponse<VehicleVersionResponse>> AddVersionAsync(Guid vehicleId, VehicleVersionRequest request)
        {
            var version = _mapper.Map<VehicleVersion>(request);
            version.VehicleId = vehicleId;
            var added = await _repo.AddVersionAsync(version);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(added), "Version created");
        }

        public async Task<ApiResponse<VehicleVersionResponse>> UpdateVersionAsync(Guid vehicleVersionId, VehicleVersionRequest request)
        {
            var version = _mapper.Map<VehicleVersion>(request);
            version.VehicleVersionId = vehicleVersionId;
            var updated = await _repo.UpdateVersionAsync(version);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(updated), "Version updated");
        }

        public async Task<ApiResponse<VehicleVersionResponse>> GetVersionByIdAsync(Guid id)
        {
            var version = await _repo.GetVersionByIdAsync(id);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(version));
        }
    }
}
