using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;
using System;

namespace ProductService.Repositories
{
    public class DealerRepository
    {
        private readonly MyDbContext _context;

        public DealerRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Dealer>> GetAllAsync()
        {
            return await _context.Dealers.ToListAsync();
        }

        public async Task<Dealer?> GetByIdAsync(Guid id)
        {
            return await _context.Dealers.FindAsync(id);
        }

        public async Task<Dealer> AddAsync(Dealer dealer)
        {
            dealer.CreatedAt = DateTime.Now;
            dealer.UpdatedAt = DateTime.Now;
            _context.Dealers.Add(dealer);
            await _context.SaveChangesAsync();
            return dealer;
        }

        public async Task<Dealer> UpdateAsync(Dealer dealer)
        {
            dealer.UpdatedAt = DateTime.Now;
            _context.Dealers.Update(dealer);
            await _context.SaveChangesAsync();
            return dealer;
        }

        public async Task<List<DealerTarget>> GetTargetsAsync(Guid dealerId)
        {
            return await _context.DealerTargets
                .Where(t => t.DealerId == dealerId)
                .ToListAsync();
        }
    }
}
