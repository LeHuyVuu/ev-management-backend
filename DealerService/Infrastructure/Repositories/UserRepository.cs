using DealerService.Context;
using DealerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealerService.Infrastructure.Repositories;

public class UserRepository
{
    private readonly MyDbContext _myDbContext;

    public UserRepository(MyDbContext myDbContext)
    {
        _myDbContext = myDbContext;
    }

    public async Task<IEnumerable<User>> GetUserWithRolDealerStaff()
    {
        return await _myDbContext.Users.Where(u => u.RoleId == 4).ToListAsync();
    }
}