using CustomerService.Context;
using CustomerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories
{
    public class PromotionRepository
    {
        private readonly MyDbContext _dbContext;
        public PromotionRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<object>> GetPromotionById(List<Guid> promotionIds)
        {
            if (promotionIds == null || !promotionIds.Any())
            {
                return Enumerable.Empty<object>();
            }

            var promotions = await _dbContext.Promotions
                .Where(p => promotionIds.Contains(p.PromotionId))
                .ToListAsync();

            return promotions;
        }
    }
}
