using Microsoft.EntityFrameworkCore;
using BrandService.Context;
using BrandService.Entities;
using System;

namespace BrandService.Infrastructure.Repositories
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
            try
            {
                return await _context.Dealers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}", ex);
            }
        }

        public async Task<Dealer?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Dealers.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}", ex);
            }
        }

        public async Task<Dealer> AddAsync(Dealer dealer)
        {
            try
            {
                dealer.CreatedAt = DateTime.Now;
                dealer.UpdatedAt = DateTime.Now;
                _context.Dealers.Add(dealer);
                await _context.SaveChangesAsync();
                return dealer;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}", ex);
            }
        }

        public async Task<Dealer> UpdateAsync(Dealer dealer)
        {
            try
            {
                dealer.UpdatedAt = DateTime.Now;
                _context.Dealers.Update(dealer);
                await _context.SaveChangesAsync();
                return dealer;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}", ex);
            }
        }

        //public async Task<List<DealerTarget>> GetTargetsAsync(Guid dealerId)
        //{
        //    return await _context.DealerTargets
        //        .Where(t => t.DealerId == dealerId)
        //        .ToListAsync();
        //}
    }
}
