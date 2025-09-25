using AutoMapper;
using ProductService.DTOs;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

public class ContractService
{
    private readonly ContractRepository _contractRepository;
    private readonly IMapper _mapper;

    public ContractService(ContractRepository contractRepository,  IMapper mapper)
    {
        _contractRepository = contractRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ContractCustomerResponse>> GetAllContractsByCustomerId(Guid customerId)
    {
        var contracts = await _contractRepository.GetAllContractsByCustomerId(customerId);
        return _mapper.Map<IEnumerable<ContractCustomerResponse>>(contracts);
    }
}