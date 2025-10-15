using CustomerService.ExceptionHandler;
using AutoMapper;
using CustomerService.DTOs.Requests.ContractDTOs;
using CustomerService.DTOs.Responses.ContractDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;

namespace CustomerService.Infrastructure.Services;

public class ContractService
{
    private readonly ContractRepository _contractRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly QuoteRepository _quoteRepository;
    private readonly DealerRepository _dealerRepository;
    private readonly UserRepository _userRepository;
    private readonly IMapper _mapper;

    public ContractService(
        ContractRepository contractRepository, 
        CustomerRepository customerRepository, 
        QuoteRepository quoteRepository,
        DealerRepository dealerRepository,
        UserRepository userRepository,
        IMapper mapper)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
        _quoteRepository = quoteRepository;
        _dealerRepository = dealerRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ContractCustomerResponse>> GetAllContractsByCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId không hợp lệ (Guid.Empty)");

        bool isExist = await _customerRepository.CustomerExists(customerId);
        if (!isExist)
            throw new KeyNotFoundException($"Customer với ID [{customerId}] không tồn tại.");

        var contracts = await _contractRepository.GetAllContractsByCustomerId(customerId);
        if (contracts == null || !contracts.Any())
            return Enumerable.Empty<ContractCustomerResponse>();

        var contractResponses = _mapper.Map<IEnumerable<ContractCustomerResponse>>(contracts);

        foreach (var contract in contractResponses)
        {
            if (contract == null)
                continue;

            var quote = await _quoteRepository.GetQuoteByContractId(contract.ContractId);
            if (quote == null)
                continue;

            var vehicle = await _quoteRepository.GetVehicleByQuoteId(quote.QuoteId);
            var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quote.QuoteId);
            var user = await _userRepository.GetUserStaffByDealerId(quote.DealerId);

            contract.StaffContract = user?.Name ?? "No assigned staff";
            contract.Brand = vehicle?.Brand ?? "Unknown";
            contract.VehicleName = vehicle?.ModelName ?? "Unknown";
            contract.VersionName = vehicleVersion?.VersionName ?? "No version information";
        }

        return contractResponses;
    }


    public async Task<bool> CreateContract(ContractCreateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "The contract creation request cannot be null.");

        if (request.QuoteId == Guid.Empty)
            throw new ArgumentException("QuoteId is required and cannot be empty.", nameof(request.QuoteId));

        var quote = await _quoteRepository.GetQuoteByQuoteId(request.QuoteId);
        if (quote == null)
            throw new KeyNotFoundException($"No quote found with ID {request.QuoteId}.");

        var contract = _mapper.Map<Contract>(request);
        if (contract == null)
            throw new InvalidOperationException("Failed to map ContractCreateRequest to Contract.");

        if (quote.TotalPrice == null)
            throw new InvalidOperationException($"Quote with ID {request.QuoteId} does not have a total price.");

        contract.TotalValue = quote.TotalPrice;
        contract.SignedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var isCreated = await _contractRepository.CreateContract(contract);
        if (!isCreated)
            throw new InvalidOperationException("Failed to create the contract in the repository.");

        return true;
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
            CustomerPhone = customer.Phone ?? "Customer don't have phone number",
            CustomerEmail = customer.Email ??  "Customer don't have email",
            DealerName = dealer.Name,
            DealerEmail = dealer.ContactEmail ??  "Dealer don't have email",
            DealerPhone = dealer.ContactPhone  ??  "Dealer don't have phone number",
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
    
    public async Task<IEnumerable<ContractDealerResponse>> GetAllContractsByDealerId(Guid dealerId)
    {
        var dealer = await _dealerRepository.GetDealerByDealerId(dealerId);
        if (dealer == null)
            throw new KeyNotFoundException("Dealer does not exist!");

        var contracts = await _contractRepository.GetAllContractsByDealerId(dealerId);
        if (contracts == null || !contracts.Any())
            return Enumerable.Empty<ContractDealerResponse>();

        var contractResponses = _mapper.Map<IEnumerable<ContractDealerResponse>>(contracts) 
                                ?? Enumerable.Empty<ContractDealerResponse>();

        foreach (var contract in contractResponses)
        {
            var quote = await _quoteRepository.GetQuoteByContractId(contract.ContractId);
            if (quote != null)
            {
                var customer = await _quoteRepository.GetCustomerByQuoteId(quote.QuoteId);
                var vehicle = await _quoteRepository.GetVehicleByQuoteId(quote.QuoteId);
                var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quote.QuoteId);

                contract.CustomerName = customer?.Name ?? "Unknown";
                contract.CustomerPhone = customer?.Phone ?? "Unknown";
                contract.Brand = vehicle?.Brand ?? "Unknown";
                contract.VehicleName = vehicle?.ModelName ?? "Unknown";
                contract.VersionName = vehicleVersion?.VersionName ?? "No version information";
            }
        }

        return contractResponses;
    }
}