using AutoMapper;
using BrandService.DTOs.Requests.InventoryDTOs;
using BrandService.DTOs.Responses.InventoryDTOs;
using BrandService.Infrastructure.Repositories;
using BrandService.Model;
using BrandService.Models;

namespace BrandService.Infrastructure.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository _repo;
        private readonly IMapper _mapper;

        public InventoryService(InventoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<InventoryResponse>> GetByIdAsync(Guid id)
        {
            var inventory = await _repo.GetByIdAsync(id);
            var mapped = _mapper.Map<InventoryResponse>(inventory);
            return ApiResponse<InventoryResponse>.Success(mapped);
        }

        public async Task<ApiResponse<PagedResult<InventoryResponse>>> GetPagedAsync(int pageNumber, int pageSize, string? searchValue)
        {
            var paged = await _repo.GetPagedAsync(pageNumber, pageSize, searchValue);
            var mapped = _mapper.Map<List<InventoryResponse>>(paged.Items);

            return ApiResponse<PagedResult<InventoryResponse>>.Success(new PagedResult<InventoryResponse>
            {
                Items = mapped,
                TotalItems = paged.TotalItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
            });
        }

        public async Task<ApiResponse<PagedResult<InventoryResponse>>> GetByDealerAsync(Guid dealerId, int pageNumber, int pageSize, string? searchValue)
        {
            var paged = await _repo.GetByDealerAsync(dealerId, pageNumber, pageSize, searchValue);
            var mapped = _mapper.Map<List<InventoryResponse>>(paged.Items);

            return ApiResponse<PagedResult<InventoryResponse>>.Success(new PagedResult<InventoryResponse>
            {
                Items = mapped,
                TotalItems = paged.TotalItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
            });
        }

        public Task UpdateDealerStockAsync(Guid dealerId, Guid versionId, int deltaQuantity)
        {
            return _repo.UpdateDealerStockAsync(dealerId, versionId, deltaQuantity);
        }
    }
}
