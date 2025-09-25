using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

public class ContractRepository
{
    private readonly MyDbContext _dbContext;

    public ContractRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Contract>> GetAllContractsByCustomerId(Guid customerId)
    {
        return await _dbContext.Contracts.Where(c => c.CustomerId == customerId).ToListAsync();
    }
}