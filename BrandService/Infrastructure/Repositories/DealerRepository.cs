using Application.ExceptionHandler;
using BrandService.Context;
using BrandService.Entities;
using BrandService.Extensions.Query;
using BrandService.Model;
using Microsoft.EntityFrameworkCore;
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

        public async Task<PagedResult<Dealer>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Dealers
                .AsNoTracking()
                .OrderByDescending(d => d.DealerCode)
                .ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<List<Dealer>> GetAllActiveDealersAsync()
        {
            try
            {
                return await _context.Dealers
                    .Where(d => d.Status.ToLower() == "active")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<Dealer?> GetByIdAsync(Guid id)
        {
            try
            {
                var dealer =  await _context.Dealers.FindAsync(id);
                if (dealer == null)
                {
                    throw new NotFoundException("Dealer not found");
                }
                return dealer;
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException("Dealer not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<Dealer> AddAsync(Dealer dealer)
        {
            try
            {

                if (DoesDealerExist(dealer.DealerCode))
                    throw new BadRequestException("Dealer code has already existed.");


                _context.Dealers.Add(dealer);
                await _context.SaveChangesAsync();
                return dealer;
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<Dealer> UpdateAsync(Dealer dealer)
        {
            try
            {

                if (DoesDealerExist(dealer.DealerCode, dealer.DealerId))
                    throw new Exception("Dealer has already exist");

                _context.Dealers.Update(dealer);
                await _context.SaveChangesAsync();
                return dealer;
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public bool DoesDealerExist(string dealerCode, Guid? excludeDealerId = null)
        {
            return _context.Dealers.AsNoTracking().Any(d => d.DealerCode == dealerCode && (excludeDealerId == null || d.DealerId != excludeDealerId));
        }

        //public async Task<List<DealerTarget>> GetTargetsAsync(Guid dealerId)
        //{
        //    return await _context.DealerTargets
        //        .Where(t => t.DealerId == dealerId)
        //        .ToListAsync();
        //}
    }
}
