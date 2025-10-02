using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.DTOs;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

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