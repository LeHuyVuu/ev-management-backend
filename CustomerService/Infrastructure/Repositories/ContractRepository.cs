using Microsoft.EntityFrameworkCore;
using CustomerService.Context;
using CustomerService.Entities;

namespace CustomerService.Infrastructure.Repositories;

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

    public async Task<Contract> GetContractByContractIncludeQuote(Guid contractId)
    {
        return await _dbContext.Contracts.Include(c => c.Quote).FirstOrDefaultAsync(c => c.ContractId == contractId);
    }
    public async Task<IEnumerable<Contract>> GetAllContractsByDealerId(Guid dealerId)
    {
        return await _dbContext.Contracts.Where(c => c.DealerId == dealerId).ToListAsync();
    }

    public async Task<bool> UpdateContractStatus(Contract contract)
    {
        _dbContext.Update(contract);
        return await _dbContext.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<Contract>> GetAllInConfirmedContractsByCustomerId(Guid customerId)
    {
        return await _dbContext.Contracts.Where(c => c.CustomerId == customerId && c.Status != "confirmed").ToListAsync();
    }
    
    public async Task<bool> UpdateContract(Contract contract)
    {
        _dbContext.Update(contract);
        return await _dbContext.SaveChangesAsync() > 0;
    }
}