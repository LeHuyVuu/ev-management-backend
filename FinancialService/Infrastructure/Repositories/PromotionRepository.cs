using FinancialService.Context;
using FinancialService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repositories;

public class PromotionRepository
{
    private readonly MyDbContext _context;

    public PromotionRepository(MyDbContext context)
    {
        _context = context;
    }

    // ✅ Trả về IQueryable thay vì List
    public IQueryable<promotion> GetAll()
    {
        return _context.promotions.AsQueryable();
    }

    public async Task<promotion?> GetByIdAsync(Guid id) =>
        await _context.promotions.FindAsync(id);

    public async Task AddAsync(promotion entity)
    {
        _context.promotions.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(promotion entity)
    {
        _context.promotions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(promotion entity)
    {
        _context.promotions.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name) =>
        await _context.promotions.AnyAsync(x => x.name == name);
}