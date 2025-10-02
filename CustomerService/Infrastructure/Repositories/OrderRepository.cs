using Microsoft.EntityFrameworkCore;
using CustomerService.Context;
using CustomerService.Entities;

namespace CustomerService.Infrastructure.Repositories;

public class OrderRepository
{
    private readonly MyDbContext _dbContext;

    public OrderRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersByCustomerId(Guid customerId)
    {
        return await _dbContext.Orders.Where(o => o.CustomerId == customerId).ToListAsync();
    }
}