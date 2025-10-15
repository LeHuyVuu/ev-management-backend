using AutoMapper;
using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;
using BrandService.Model;
using BrandService.Models;
using BrandService.ExceptionHandler;

namespace BrandService.Infrastructure.Services
{
    public class VehicleService
    {
        private readonly VehicleRepository _repo;
        private readonly IMapper _mapper;

        public VehicleService(VehicleRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<VehicleResponse>>> GetPagedAsync(int page, int size)
        {
            var result = await _repo.GetPagedAsync(page, size);
            var mapped = new PagedResult<VehicleResponse>
            {
                Items = result.Items.Select(_mapper.Map<VehicleResponse>).ToList(),
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            return ApiResponse<PagedResult<VehicleResponse>>.Success(mapped);
        }

        public async Task<ApiResponse<VehicleDetailResponse>> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            var dto = _mapper.Map<VehicleDetailResponse>(entity);
            dto.Versions = entity.VehicleVersions.Select(_mapper.Map<VehicleVersionResponse>).ToList();
            return ApiResponse<VehicleDetailResponse>.Success(dto);
        }

        public async Task<ApiResponse<VehicleResponse>> AddAsync(CreateVehicleRequest request)
        {
            var entity = _mapper.Map<Vehicle>(request);
            var added = await _repo.AddAsync(entity);
            return ApiResponse<VehicleResponse>.Success(_mapper.Map<VehicleResponse>(added), "Vehicle created");
        }

        public async Task<ApiResponse<VehicleVersionResponse>> AddVersionAsync(Guid vehicleId, CreateVehicleVersionRequest request)
        {
            var version = new VehicleVersion
            {
                VehicleId = vehicleId,
                VersionName = request.VersionName,
                Color = request.Color,
                EvType = request.EvType,
                HorsePower = request.HorsePower,
                BasePrice = request.BasePrice
            };
            var added = await _repo.AddVersionAsync(version);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(added), "Version created");
        }

        public async Task<ApiResponse<VehicleVersionResponse>> GetVersionByIdAsync(Guid id)
        {
            var version = await _repo.GetVersionByIdAsync(id);
            return ApiResponse<VehicleVersionResponse>.Success(_mapper.Map<VehicleVersionResponse>(version));
        }
    }
}
