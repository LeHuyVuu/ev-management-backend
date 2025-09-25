using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

public class CustomerRepository
{
    private readonly MyDbContext _dbContext;

    public CustomerRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        return await _dbContext.Customers.ToListAsync();
    }
}