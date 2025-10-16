using AutoMapper;
using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;
using BrandService.Model;
using BrandService.Models;
using Elastic.Clients.Elasticsearch;

namespace BrandService.Infrastructure.Services
{
    public class VehicleVersionService
    {
        private readonly VehicleVersionRepository _vehicleVersionRepo;
        private readonly BrandInventoryRepository _brandInventoryRepo;
        private readonly IMapper _mapper;
        public VehicleVersionService(VehicleVersionRepository vehicleVersionRepo, BrandInventoryRepository brandInventoryRepo, IMapper mapper)
        {
            _vehicleVersionRepo = vehicleVersionRepo;
            _brandInventoryRepo = brandInventoryRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<VehicleVersionResponse>>> GetPagedAsync(int pageNumber, int pageSize, string? searchValue)
        {
            var pagedVersions = await _vehicleVersionRepo.GetPagedAsync(pageNumber, pageSize, searchValue);

            var mapped = _mapper.Map<IEnumerable<VehicleVersionResponse>>(pagedVersions.Items).ToList();

            var result = new PagedResult<VehicleVersionResponse>
            {
                Items = mapped,
                TotalItems = pagedVersions.TotalItems,
                PageNumber = pagedVersions.PageNumber,
                PageSize = pagedVersions.PageSize,
            };

            return ApiResponse<PagedResult<VehicleVersionResponse>>.Success(result);
        }

        public async Task<ApiResponse<VehicleVersionResponse>> AddVersionAsync(Guid vehicleId, VehicleVersionRequest request)
        {
            var version = _mapper.Map<VehicleVersion>(request);
            version.VehicleId = vehicleId;
            var added = await _vehicleVersionRepo.AddVersionAsync(version);
            var brandInventory = new BrandInventory
            {
                VehicleVersionId = added.VehicleVersionId,
                StockQuantity = request.StockQuantity ?? 0
            };
            await _brandInventoryRepo.AddInventoryAsync(brandInventory);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(added), "Version created");
        }

        public async Task<ApiResponse<VehicleVersionResponse>> UpdateVersionAsync(Guid vehicleVersionId, VehicleVersionRequest request)
        {
            var version = _mapper.Map<VehicleVersion>(request);
            version.VehicleVersionId = vehicleVersionId;
            var updated = await _vehicleVersionRepo.UpdateVersionAsync(version);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(updated), "Version updated");
        }

        public async Task<ApiResponse<VehicleVersionResponse>> GetVersionByIdAsync(Guid id)
        {
            var version = await _vehicleVersionRepo.GetVersionByIdAsync(id);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(version));
        }

        public async Task<ApiResponse<string>> DeleteVersionAsync(Guid id)
        {
            var deleted = await _vehicleVersionRepo.DeleteVersionAsync(id);
            return ApiResponse<string>.Success("Version deleted");
        }
    }
}
