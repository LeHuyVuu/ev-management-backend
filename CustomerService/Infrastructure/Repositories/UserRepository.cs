using CustomerService.Context;
using CustomerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

public class UserRepository
{
    private readonly MyDbContext _dbContext;

    public UserRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> GetUserStaffByDealerId(Guid dealerId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.DealerId == dealerId && u.RoleId == 4);
    }
}