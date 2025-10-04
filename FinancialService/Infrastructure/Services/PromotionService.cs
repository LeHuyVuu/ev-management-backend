using FinancialService.Context;
using FinancialService.Entities;
using FinancialService.Model;
using FinancialService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Services;

public class PromotionService
{
    private readonly PromotionRepository _repo;
    private readonly MyDbContext _context;

    public PromotionService(PromotionRepository repo, MyDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    private string GetStatus(DateOnly start, DateOnly end)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        if (now < start) return "pending";
        if (now > end) return "expired";
        return "active";
    }

   public async Task<ApiResponse<PagedResult<promotion>>> GetPagedAsync(PaginationParams p)
{
    try
    {
        var query = _repo.GetAll();

        // Filter nếu có
        if (!string.IsNullOrEmpty(p.Status))
            query = query.Where(x => x.status.ToLower() == p.Status.ToLower());
        if (!string.IsNullOrEmpty(p.Keyword))
            query = query.Where(x => x.name.ToLower().Contains(p.Keyword.ToLower()));

        // Sort
        switch (p.SortBy?.ToLower())
        {
            case "name":
                query = p.SortOrder == "desc" ? query.OrderByDescending(x => x.name) : query.OrderBy(x => x.name);
                break;
            case "type":
                query = p.SortOrder == "desc" ? query.OrderByDescending(x => x.type) : query.OrderBy(x => x.type);
                break;
            case "end_date":
                query = p.SortOrder == "desc" ? query.OrderByDescending(x => x.end_date) : query.OrderBy(x => x.end_date);
                break;
            case "status":
                query = p.SortOrder == "desc" ? query.OrderByDescending(x => x.status) : query.OrderBy(x => x.status);
                break;
            default:
                query = p.SortOrder == "desc" ? query.OrderByDescending(x => x.start_date) : query.OrderBy(x => x.start_date);
                break;
        }

        // Pagination
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)p.PageSize);
        var items = await query.Skip((p.PageNumber - 1) * p.PageSize)
                               .Take(p.PageSize)
                               .ToListAsync();

        var result = new PagedResult<promotion>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = totalPages,
            PageNumber = p.PageNumber,
            PageSize = p.PageSize
        };

        return ApiResponse<PagedResult<promotion>>.Success(result);
    }
    catch (Exception ex)
    {
        return ApiResponse<PagedResult<promotion>>.Fail(500, "Error retrieving promotions", ex.Message);
    }
}


    public async Task<ApiResponse<promotion>> GetByIdAsync(Guid id)
    {
        try
        {
            var promo = await _repo.GetByIdAsync(id);
            if (promo == null)
                return ApiResponse<promotion>.Fail(404, "Promotion not found");

            return ApiResponse<promotion>.Success(promo);
        }
        catch (Exception ex)
        {
            return ApiResponse<promotion>.Fail(500, "Error retrieving promotion", ex.Message);
        }
    }

    public async Task<ApiResponse<promotion>> CreateAsync(PromotionCreateDto dto)
    {
        try
        {
            if (await _repo.ExistsByNameAsync(dto.name))
                return ApiResponse<promotion>.Fail(400, "Promotion name already exists");

            // ✅ Parse string -> DateOnly
            var start = DateOnly.Parse(dto.start_date);
            var end = DateOnly.Parse(dto.end_date);

            var entity = new promotion
            {
                promotion_id = Guid.NewGuid(),
                name = dto.name,
                type = dto.type,
                start_date = start,
                end_date = end,
                description = dto.description,
                status = GetStatus(start, end)
            };

            await _repo.AddAsync(entity);

            return ApiResponse<promotion>.Success(entity, "Promotion created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<promotion>.Fail(500, "Error creating promotion", ex.Message);
        }
    }


    public async Task<ApiResponse<promotion>> UpdateAsync(Guid id, PromotionUpdateDto dto)
    {
        try
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<promotion>.Fail(404, "Promotion not found");

            if (existing.name != dto.name && await _repo.ExistsByNameAsync(dto.name))
                return ApiResponse<promotion>.Fail(400, "Another promotion with this name already exists");

            var start = DateOnly.Parse(dto.start_date);
            var end = DateOnly.Parse(dto.end_date);

            existing.name = dto.name;
            existing.type = dto.type;
            existing.start_date = start;
            existing.end_date = end;
            existing.description = dto.description;
            existing.status = GetStatus(start, end);

            await _repo.UpdateAsync(existing);

            return ApiResponse<promotion>.Success(existing, "Promotion updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<promotion>.Fail(500, "Error updating promotion", ex.Message);
        }
    }



    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        try
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<string>.Fail(404, "Promotion not found");

            // ✅ Thay vì xóa thật → chuyển sang soft delete
            if (existing.status.ToLower() == "deleted")
                return ApiResponse<string>.Fail(400, "Promotion already deleted");

            existing.status = "deleted";
            await _repo.UpdateAsync(existing);

            return ApiResponse<string>.Success("Promotion deleted (soft delete)");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(500, "Error deleting promotion", ex.Message);
        }
    }

}
