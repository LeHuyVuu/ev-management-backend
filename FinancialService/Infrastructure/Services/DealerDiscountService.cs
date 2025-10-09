using AutoMapper;
using FinancialService.Entities;
using FinancialService.Model;
using FinancialService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Services;

public class DealerDiscountService
{
    private readonly DealerDiscountRepository _repo;
    private readonly IMapper _mapper;

    public DealerDiscountService(DealerDiscountRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    private string GetStatus(DateOnly from, DateOnly to)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today < from) return "pending";
        if (today > to) return "expired";
        return "active";
    }

    public async Task<ApiResponse<PagedResult<DealerDiscountResponseDto>>> GetPagedAsync(PaginationParams p)
    {
        try
        {
            var query = _repo.GetAll();

            if (!string.IsNullOrEmpty(p.Status))
                query = query.Where(x => x.status.ToLower() == p.Status.ToLower());

            if (!string.IsNullOrEmpty(p.Keyword))
                query = query.Where(x => x.dealer.name.ToLower().Contains(p.Keyword.ToLower()));

            // Sort
            query = p.SortBy?.ToLower() switch
            {
                "discount_rate" => p.SortOrder == "desc" ? query.OrderByDescending(x => x.discount_rate) : query.OrderBy(x => x.discount_rate),
                "valid_from" => p.SortOrder == "desc" ? query.OrderByDescending(x => x.valid_from) : query.OrderBy(x => x.valid_from),
                "valid_to" => p.SortOrder == "desc" ? query.OrderByDescending(x => x.valid_to) : query.OrderBy(x => x.valid_to),
                "status" => p.SortOrder == "desc" ? query.OrderByDescending(x => x.status) : query.OrderBy(x => x.status),
                _ => query.OrderBy(x => x.valid_from)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)p.PageSize);

            var entities = await query
                .Skip((p.PageNumber - 1) * p.PageSize)
                .Take(p.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<List<DealerDiscountResponseDto>>(entities);

            var result = new PagedResult<DealerDiscountResponseDto>
            {
                Items = dtoList,
                TotalItems = totalItems,
                TotalPages = totalPages,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize
            };

            return ApiResponse<PagedResult<DealerDiscountResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<DealerDiscountResponseDto>>.Fail(500, "Error retrieving dealer discounts", ex.Message);
        }
    }

    public async Task<ApiResponse<DealerDiscountResponseDto>> CreateAsync(DealerDiscountCreateDto dto)
    {
        try
        {
            var from = DateOnly.Parse(dto.valid_from);
            var to = DateOnly.Parse(dto.valid_to);

            if (await _repo.ExistsAsync(dto.dealer_id, from, to))
                return ApiResponse<DealerDiscountResponseDto>.Fail(400, "Dealer already has a discount policy overlapping this period");

            var entity = _mapper.Map<dealer_discount>(dto);
            entity.dealer_discount_id = Guid.NewGuid();
            entity.status = GetStatus(entity.valid_from, entity.valid_to);

            await _repo.AddAsync(entity);
            var result = _mapper.Map<DealerDiscountResponseDto>(entity);

            return ApiResponse<DealerDiscountResponseDto>.Success(result, "Dealer discount created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<DealerDiscountResponseDto>.Fail(500, "Error creating dealer discount", ex.Message);
        }
    }

    public async Task<ApiResponse<DealerDiscountResponseDto>> UpdateAsync(Guid id, DealerDiscountUpdateDto dto)
    {
        try
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<DealerDiscountResponseDto>.Fail(404, "Dealer discount not found");

            _mapper.Map(dto, existing);
            existing.status = GetStatus(existing.valid_from, existing.valid_to);

            await _repo.UpdateAsync(existing);

            var result = _mapper.Map<DealerDiscountResponseDto>(existing);
            return ApiResponse<DealerDiscountResponseDto>.Success(result, "Dealer discount updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<DealerDiscountResponseDto>.Fail(500, "Error updating dealer discount", ex.Message);
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        try
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<string>.Fail(404, "Dealer discount not found");

            existing.status = "expired";
            await _repo.UpdateAsync(existing);

            return ApiResponse<string>.Success("Dealer discount marked as expired (soft delete)");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(500, "Error deleting dealer discount", ex.Message);
        }
    }

    public async Task<ApiResponse<string>> ActivateAsync(Guid id)
    {
        try
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<string>.Fail(404, "Dealer discount not found");

            existing.status = "active";
            await _repo.UpdateAsync(existing);

            return ApiResponse<string>.Success("Dealer discount activated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(500, "Error activating dealer discount", ex.Message);
        }
    }
}
