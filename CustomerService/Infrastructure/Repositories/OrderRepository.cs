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

    public async Task<bool> CreateOrder(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Order> GetOrderByOrderId(Guid orderId)
    {
        return await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByDealerId(Guid dealerId)
    {
        return await _dbContext.Orders
            .Include(o => o.Contract)
                .ThenInclude(c => c.Customer)
            .Include(o => o.Contract)
                .ThenInclude(c => c.Quote)
                .ThenInclude(q => q.VehicleVersion)
                .ThenInclude(vv => vv.Vehicle)
            .Where(o => o.DealerId == dealerId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatus(Order order)
    {
        _dbContext.Update(order);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}