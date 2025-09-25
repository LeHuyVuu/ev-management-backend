using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.DTOs;
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

    public async Task<Customer> GetCustomerDetail(Guid customerId)
    {
        return await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<bool> CreateCustomer(Customer customer)
    {
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}