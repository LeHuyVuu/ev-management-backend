using AutoMapper;
using BrandService.DTOs.Requests.InventoryDTOs;
using BrandService.DTOs.Responses.InventoryDTOs;
using BrandService.Infrastructure.Repositories;
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

        public async Task<ApiResponse<List<InventoryResponse>>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            var data = list.Select(_mapper.Map<InventoryResponse>).ToList();
            return ApiResponse<List<InventoryResponse>>.Success(data);
        }

        public async Task<ApiResponse<List<InventoryResponse>>> GetByDealerAsync(Guid dealerId)
        {
            var list = await _repo.GetByDealerAsync(dealerId);
            var data = list.Select(_mapper.Map<InventoryResponse>).ToList();
            return ApiResponse<List<InventoryResponse>>.Success(data);
        }

        public async Task<ApiResponse<InventoryResponse>> UpdateStockAsync(Guid id, UpdateInventoryRequest req)
        {
            var updated = await _repo.UpdateStockAsync(id, req.StockQuantity);
            return ApiResponse<InventoryResponse>.Success(_mapper.Map<InventoryResponse>(updated), "Stock updated");
        }
    }
}
