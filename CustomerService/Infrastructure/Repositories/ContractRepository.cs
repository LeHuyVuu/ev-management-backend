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

    public async Task<bool> CreateContract(Contract contract)
    {
        await _dbContext.Contracts.AddAsync(contract);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<Contract> GetContractByContractId(Guid contractId)
    {
        return await _dbContext.Contracts.FirstOrDefaultAsync(c => c.ContractId == contractId);
    }

    public async Task<IEnumerable<Contract>> GetAllContractsByDealerId(Guid dealerId)
    {
        return await _dbContext.Contracts.Where(c => c.DealerId == dealerId).ToListAsync();
    }
}