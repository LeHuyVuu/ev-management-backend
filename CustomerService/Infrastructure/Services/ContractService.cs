using Application.ExceptionHandler;
using AutoMapper;
using ProductService.DTOs;
using ProductService.DTOs.Requests.ContractDTOs;
using ProductService.Entities;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

public class ContractService
{
    private readonly ContractRepository _contractRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly QuoteRepository _quoteRepository;
    private readonly DealerRepository _dealerRepository;
    private readonly IMapper _mapper;

    public ContractService(
        ContractRepository contractRepository, 
        CustomerRepository customerRepository, 
        QuoteRepository quoteRepository,
        DealerRepository dealerRepository,
        IMapper mapper)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
        _quoteRepository = quoteRepository;
        _dealerRepository = dealerRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ContractCustomerResponse>> GetAllContractsByCustomerId(Guid customerId)
    {
        bool isExist = await _customerRepository.CustomerExists(customerId);
        if (!isExist)
            throw new KeyNotFoundException("Customer does not exist!");
        
        var contracts = await _contractRepository.GetAllContractsByCustomerId(customerId);
        var contractResponses = _mapper.Map<IEnumerable<ContractCustomerResponse>>(contracts);

        foreach (var contract in contractResponses)
        {
            var quote = await _quoteRepository.GetQuoteByContractId(contract.ContractId);
            if (quote != null)
            {
                var vehicle = await _quoteRepository.GetVehicleByQuoteId(quote.QuoteId);
                var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quote.QuoteId);
                
                contract.Brand = vehicle.Brand;
                contract.VehicleName = vehicle.ModelName;
                contract.VersionName = vehicleVersion.VersionName;
            }
        }
        
        return contractResponses;
    }

    public async Task<bool> CreateContract(ContractCreateRequest request)
    {
        var quote = await _quoteRepository.GetQuoteByQuoteId(request.QuoteId);
        if (quote == null)
            throw new NotFoundException("Quote does not exist!");

        var contract = _mapper.Map<Contract>(request);
        contract.TotalValue = quote.TotalPrice;
        contract.SignedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        return await _contractRepository.CreateContract(contract);
    }
    
    public async Task<ContractDetailResponse> GetContractByContractId(Guid contractId)
    {
        var contract = await _contractRepository.GetContractByContractId(contractId);
        if(contract == null)
            throw new NotFoundException("Contract not found!");

        var quote = await _quoteRepository.GetQuoteByContractId(contractId);
        if(quote == null)
            throw new NotFoundException("Quote not found!");

        var customer = await _quoteRepository.GetCustomerByQuoteId(quote.QuoteId);
        if (customer == null)
            throw new NotFoundException($"Customer not found");

        var vehicle = await _quoteRepository.GetVehicleByQuoteId(quote.QuoteId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle not found");

        var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quote.QuoteId);
        if (vehicleVersion == null)
            throw new NotFoundException($"Vehicle not found");
        
        var dealer = await _dealerRepository.GetDealerByDealerId(quote.DealerId);
        if (dealer == null)
            throw new NotFoundException($"Dealer not found");

        var contractResponse = new ContractDetailResponse()
        {
            ContractId = contractId,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            CustomerEmail = customer.Email,
            DealerName = dealer.Name,
            DealerEmail = dealer.ContactEmail,
            DealerPhone = dealer.ContactPhone,
            Brand = vehicle.Brand,
            VehicleName = vehicle.ModelName,
            VersionName = vehicleVersion.VersionName,
            TotalValue = contract.TotalValue,
            PaymentMethod = contract.PaymentMethod,
            Status = contract.Status,
            SignedDate = contract.SignedDate ?? DateOnly.MinValue
        };
        return contractResponse;
    }
}