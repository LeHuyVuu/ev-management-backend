using DealerService.Models;
using DealerService.Extensions.Query;
using Microsoft.EntityFrameworkCore;
using DealerService.Context;
using DealerService.Entities;
using DealerService.ExceptionHandler;

namespace DealerService.Infrastructure.Repositories
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
                var dealer = await _context.Dealers.FindAsync(id);
                if (dealer == null)
                {
                    throw new NotFoundException("Dealer not found");
                }
                return dealer;
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
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
                    throw new BadRequestException("Dealer has already exist");

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
    }
}
