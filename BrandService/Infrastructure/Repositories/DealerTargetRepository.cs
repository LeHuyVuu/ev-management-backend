using Application.ExceptionHandler;
using BrandService.Context;
using BrandService.Entities;
using BrandService.Extensions.Query;
using BrandService.Model;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class DealerTargetRepository
    {
        private readonly MyDbContext _context;
        private readonly DealerRepository _dealerRepository;

        public DealerTargetRepository(MyDbContext context, DealerRepository dealerRepository)
        {
            _context = context;
            _dealerRepository = dealerRepository;
        }

        public async Task<PagedResult<DealerTarget>> GetPagedByDealerAsync(Guid dealerId, int pageNumber, int pageSize)
        {
            try
            {
                var dealer = await _dealerRepository.GetByIdAsync(dealerId);
                if (dealer == null)
                    throw new NotFoundException("Dealer not found");
                return await _context.DealerTargets
                    .Where(t => t.DealerId == dealerId)
                    .AsNoTracking()
                    .OrderByDescending(t => t.StartDate)
                    .ToPagedResultAsync(pageNumber, pageSize);
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

        public async Task<DealerTarget?> GetByIdAsync(Guid dealerId, Guid targetId)
        {
            try
            {
                var dealer = await _dealerRepository.GetByIdAsync(dealerId);
                if (dealer == null)
                    throw new NotFoundException("Dealer not found");
                var target = await _context.DealerTargets
                    .FirstOrDefaultAsync(t => t.DealerTargetId == targetId && t.DealerId == dealerId);
                if (target == null)
                    throw new NotFoundException("Dealer target not found");
                return target;
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

        public async Task<DealerTarget> AddAsync(DealerTarget target)
        {
            try
            {
                var dealer = await _dealerRepository.GetByIdAsync(target.DealerId);
                if (dealer == null)
                    throw new NotFoundException("Dealer not found");

                var overlap = await ExistsOverlapAsync(target.DealerId, null, target.StartDate, target.Period);
                if (overlap)
                    throw new BadRequestException("Target period overlaps an existing target for this dealer.");

                target.DealerTargetId = Guid.NewGuid();
                _context.DealerTargets.Add(target);
                await _context.SaveChangesAsync();
                return target;
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<DealerTarget> UpdateAsync(DealerTarget target)
        {
            try
            {
                var dealer = await _dealerRepository.GetByIdAsync(target.DealerId);
                if (dealer == null)
                    throw new NotFoundException("Dealer not found");

                var overlap = await ExistsOverlapAsync(target.DealerId, target.DealerTargetId, target.StartDate, target.Period);
                if (overlap)
                    throw new BadRequestException("Target period overlaps an existing target for this dealer.");

                var existingTarget = await _context.DealerTargets
                    .FirstOrDefaultAsync(t => t.DealerTargetId == target.DealerTargetId && t.DealerId == target.DealerId);
                if (existingTarget == null)
                    throw new NotFoundException("Dealer target not found");
                _context.Entry(existingTarget).CurrentValues.SetValues(target);
                await _context.SaveChangesAsync();
                return existingTarget;
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

        public async Task DeleteAsync(DealerTarget target)
        {
            try
            {
                var dealer = await _dealerRepository.GetByIdAsync(target.DealerId);
                if (dealer == null)
                    throw new NotFoundException("Dealer not found");
                var existingTarget = await _context.DealerTargets
                    .FirstOrDefaultAsync(t => t.DealerTargetId == target.DealerTargetId && t.DealerId == target.DealerId);
                if (existingTarget == null)
                    throw new NotFoundException("Dealer target not found");
                _context.DealerTargets.Remove(existingTarget);
                await _context.SaveChangesAsync();
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

        private async Task<bool> ExistsOverlapAsync(Guid dealerId, Guid? excludeTargetId, DateOnly newStartDate, string newPeriod)
        {
            var targets = await _context.DealerTargets
                .AsNoTracking()
                .Where(t => t.DealerId == dealerId && (excludeTargetId == null || t.DealerTargetId != excludeTargetId))
                .ToListAsync();

            var newEndDate = newPeriod.ToLower() switch
            {
                "monthly" => newStartDate.AddMonths(1),
                "quarterly" => newStartDate.AddMonths(3),
                "yearly" => newStartDate.AddYears(1),
                _ => newStartDate
            };

            return targets.Any(t =>
            {
                var existingEndDate = t.Period.ToLower() switch
                {
                    "monthly" => t.StartDate.AddMonths(1),
                    "quarterly" => t.StartDate.AddMonths(3),
                    "yearly" => t.StartDate.AddYears(1),
                    _ => t.StartDate
                };

                return t.StartDate < newEndDate && newStartDate < existingEndDate;
            });
        }


        /// <summary>
        /// Tính tổng doanh số (total_value) từ bảng contracts cho 1 dealer trong kỳ target.
        /// </summary>
        public async Task<decimal> CalculateAchievedAmountAsync(Guid dealerId, DateOnly startDate, string period)
        {
            try
            {
                var query = _context.Contracts
                .Where(c => c.DealerId == dealerId &&
                            (c.Status == "approved" || c.Status == "completed") &&
                            c.SignedDate != null);

                // Monthly
                if (period.ToLower() == "monthly")
                {
                    query = query.Where(c =>
                        c.SignedDate.Value.Month == startDate.Month &&
                        c.SignedDate.Value.Year == startDate.Year);
                }

                // Quarterly
                if (period.ToLower() == "quarterly")
                {
                    var quarter = (startDate.Month - 1) / 3 + 1;
                    query = query.Where(c =>
                        ((c.SignedDate.Value.Month - 1) / 3 + 1) == quarter &&
                        c.SignedDate.Value.Year == startDate.Year);
                }

                // Yearly
                if (period.ToLower() == "yearly")
                {
                    query = query.Where(c => c.SignedDate.Value.Year == startDate.Year);
                }

                return await query.SumAsync(c => c.TotalValue);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
