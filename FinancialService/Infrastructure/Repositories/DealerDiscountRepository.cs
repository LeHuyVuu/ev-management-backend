using FinancialService.Context;
using FinancialService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repositories;

public class DealerDiscountRepository
{
    private readonly MyDbContext _context;

    public DealerDiscountRepository(MyDbContext context)
    {
        _context = context;
    }

    public IQueryable<dealer_discount> GetAll()
        => _context.dealer_discounts.Include(x => x.dealer).AsQueryable();

    public async Task<dealer_discount?> GetByIdAsync(Guid id)
        => await _context.dealer_discounts.Include(x => x.dealer)
            .FirstOrDefaultAsync(x => x.dealer_discount_id == id);

    public async Task AddAsync(dealer_discount entity)
    {
        _context.dealer_discounts.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(dealer_discount entity)
    {
        _context.dealer_discounts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid dealerId, DateOnly from, DateOnly to)
        => await _context.dealer_discounts.AnyAsync(x =>
            x.dealer_id == dealerId &&
            x.valid_from <= to &&
            x.valid_to >= from);
}