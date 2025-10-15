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
            dto.Versions = entity.VehicleVersions.Select(_mapper.Map<BrandVehicleVersionResponse>).ToList();
            return ApiResponse<VehicleDetailResponse>.Success(dto);
        }

        public async Task<ApiResponse<VehicleResponse>> AddAsync(VehicleRequest request)
        {
            var entity = _mapper.Map<Vehicle>(request);
            var added = await _repo.AddAsync(entity);
            return ApiResponse<VehicleResponse>.Success(_mapper.Map<VehicleResponse>(added), "Vehicle created");
        }

        public async Task<ApiResponse<VehicleResponse>> UpdateAsync(Guid id, VehicleRequest request)
        {
            var entity = _mapper.Map<Vehicle>(request);
            entity.VehicleId = id;
            var updated = await _repo.UpdateAsync(entity);
            return ApiResponse<VehicleResponse>.Success(_mapper.Map<VehicleResponse>(updated), "Vehicle updated");
        }
    }
}
