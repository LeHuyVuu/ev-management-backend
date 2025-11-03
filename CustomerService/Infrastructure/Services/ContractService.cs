using CustomerService.ExceptionHandler;
using AutoMapper;
using CustomerService.DTOs.Requests.ContractDTOs;
using CustomerService.DTOs.Responses.ContractDTOs;
using CustomerService.Entities;
using CustomerService.Extensions.Helper;
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


    public async Task<bool> CreateContract(Guid dealerId, ContractCreateRequest request)
    {
        if(dealerId == Guid.Empty)
            throw new KeyNotFoundException("You don't have an dealer Id to access this function.");
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
        contract.DealerId = dealerId;

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
        
        var progress = ProgressPercent.Get(contract.TotalValue, contract.AmountPaid ?? 0);

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
            FileUrl = contract.FileUrl ?? "Contract don't have file",
            TotalValue = contract.TotalValue,
            PaymentMethod = contract.PaymentMethod,
            ProgressPercent = progress,
            AmountPaid = contract.AmountPaid ?? 0m,
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
    
    public async Task<bool> CreateContractByQuotesStatus(Guid dealerId, Guid quoteId, Guid customerId)
    {
        if(dealerId == Guid.Empty)
            throw new KeyNotFoundException("You don't have an dealer Id to access this function.");

        if (quoteId == Guid.Empty)
            throw new ArgumentException("QuoteId is required and cannot be empty.", nameof(quoteId));

        var quote = await _quoteRepository.GetQuoteByQuoteId(quoteId);
        if (quote == null)
            throw new KeyNotFoundException($"No quote found with ID {quoteId}.");

        var contract = new Contract
        {
            QuoteId = quoteId,
            CustomerId = customerId,
            FileUrl = null,
            PaymentMethod = "Not selected yet",
            Status = "draft",
        };
        if (contract == null)
            throw new InvalidOperationException("Failed to map ContractCreateRequest to Contract.");

        if (quote.TotalPrice == null)
            throw new InvalidOperationException($"Quote with ID {quoteId} does not have a total price.");

        contract.TotalValue = quote.TotalPrice;
        contract.SignedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        contract.DealerId = dealerId;

        var isCreated = await _contractRepository.CreateContract(contract);
        if (!isCreated)
            throw new InvalidOperationException("Failed to create the contract in the repository.");

        return true;
    }

    public async Task<bool> UpdateContractStatus(Guid dealerId, Guid contractId, ContractUpdateStatusRequest request)
    {
        if (dealerId == Guid.Empty)
            throw new KeyNotFoundException("You don't have an dealer Id to access this function.");
        if (contractId == Guid.Empty)
            throw new ArgumentException("ContractId is required and cannot be empty.", nameof(contractId));
        if (request == null)
            throw new ArgumentNullException(nameof(request), "The contract update request cannot be null.");

        var contract = await _contractRepository.GetContractByContractId(contractId);
        if (contract == null)
            throw new NotFoundException("Contract not found!");

        if (contract.DealerId != dealerId)
            throw new UnauthorizedAccessException("Dealer does not own this contract");

        var allowedStatuses = new[] { "draft", "confirmed", "approved" };
        var normalizedStatus = request.Status?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedStatus))
            throw new InvalidOperationException("Status cannot be empty.");
        if (!allowedStatuses.Contains(normalizedStatus))
            throw new InvalidOperationException("Invalid status value. Only accepted: draft, confirmed, approved.");

        if (string.Equals(contract.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase))
            return true;
        
        if (contract.Status == "approved")
            throw new InvalidOperationException("Cannot change status of an approved contract.");

        if (normalizedStatus == "confirmed" && contract.SignedDate == null)
        {
            contract.SignedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        contract.Status = normalizedStatus;

        var updated = await _contractRepository.UpdateContractStatus(contract);
        return updated;
    }
    
    public async Task<bool> UpdateContractFile(Guid dealerId, Guid contractId, ContractUpdateFileRequest request)
    {
        if (dealerId == Guid.Empty)
            throw new KeyNotFoundException("You don't have an dealer Id to access this function.");
        if (contractId == Guid.Empty)
            throw new ArgumentException("ContractId is required and cannot be empty.", nameof(contractId));
        if (request == null)
            throw new ArgumentNullException(nameof(request), "The contract update request cannot be null.");

        var contract = await _contractRepository.GetContractByContractId(contractId);
        if (contract == null)
            throw new NotFoundException("Contract not found!");

        if (contract.DealerId != dealerId)
            throw new UnauthorizedAccessException("Dealer does not own this contract");

        if(request.FileUrl == null)
            throw new InvalidOperationException("FileUrl is required and cannot be null.");
        contract.FileUrl = request.FileUrl;
        var updated = await _contractRepository.UpdateContractStatus(contract);
        return updated;
    }
    
    public async Task<IEnumerable<ContractCustomerResponse>> GetAllInConfirmedContractsByCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId không hợp lệ (Guid.Empty)");

        bool isExist = await _customerRepository.CustomerExists(customerId);
        if (!isExist)
            throw new KeyNotFoundException($"Customer với ID [{customerId}] không tồn tại.");

        var contracts = await _contractRepository.GetAllInConfirmedContractsByCustomerId(customerId);
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

   public async Task<bool> UpdateContract(Guid dealerId, Guid contractId, ContractUpdateRequest request)
    {
        var contract = await _contractRepository.GetContractByContractId(contractId);
        if (contract == null)
            throw new NotFoundException("Không tìm thấy hợp đồng!");

        if (contract.DealerId != dealerId)
            throw new InvalidOperationException("Đại lý của bạn không có quyền xử lý hợp đồng này.");

        var reqMethod = (request.PaymentMethod ?? "").Trim().ToLowerInvariant();
        if (reqMethod is not ("installment" or "bank_transfer" or "cash"))
            throw new InvalidOperationException("Phương thức thanh toán không được hỗ trợ.");

        var current = (contract.PaymentMethod ?? "").Trim();

        decimal total = contract.TotalValue;
        decimal oldPaid = contract.AmountPaid ?? 0m;
        decimal newPaid = request.NewAmountPaid;

        // --- Case 1: Chưa chọn method (mặc định) ---
        if (string.Equals(current, "Not selected yet", StringComparison.OrdinalIgnoreCase))
        {
            if (reqMethod == "installment")
            {
                if (request.Months <= 0 || request.Pct <= 0 || request.Pct > 100)
                    throw new InvalidOperationException("Số tháng hoặc phần trăm trả góp không hợp lệ.");

                contract.PaymentMethod = InstallmentCodec.Build(v: 1, months: request.Months, pct: request.Pct);

                var per = Math.Round(total * request.Pct / 100m, 0, MidpointRounding.AwayFromZero);
                if (newPaid == 0m)
                {
                    contract.AmountPaid = 0m;
                }
                else
                {
                    var minNeed = Math.Min(per, total);
                    var maxNeed = total;
                    if (newPaid < minNeed || newPaid > maxNeed)
                        throw new InvalidOperationException(
                            $"Khoản thanh toán đầu tiên phải ít nhất {minNeed:N0} VND và tối đa {maxNeed:N0} VND.");
                    contract.AmountPaid = newPaid;
                }

                return await _contractRepository.UpdateContract(contract);
            }
            else
            {
                if (newPaid < 0m || newPaid > total)
                    throw new InvalidOperationException("Số tiền thanh toán không hợp lệ.");

                contract.PaymentMethod = reqMethod; // "cash" | "bank_transfer"
                contract.AmountPaid = newPaid;

                return await _contractRepository.UpdateContract(contract);
            }
        }

        // --- Case 2: Đang ở installment ---
        if (current.StartsWith("installment?", StringComparison.OrdinalIgnoreCase))
        {
            if (reqMethod != "installment")
                throw new InvalidOperationException("Không thể đổi phương thức từ trả góp sang hình thức khác.");

            var (_, _, pct) = InstallmentCodec.Parse(current);
            var per = Math.Round(total * pct / 100m, 0, MidpointRounding.AwayFromZero);

            if (newPaid < oldPaid)
                throw new InvalidOperationException("Số tiền mới không được nhỏ hơn số tiền hiện tại.");
            if (newPaid > total)
                throw new InvalidOperationException("Số tiền mới vượt quá tổng giá trị hợp đồng.");

            var remainingBefore = Math.Max(total - oldPaid, 0m);
            if (remainingBefore == 0m)
                throw new InvalidOperationException("Hợp đồng này đã được thanh toán đầy đủ.");

            var minNeed = Math.Min(per, remainingBefore);
            var delta = newPaid - oldPaid;

            // Cho phép trả >= tối thiểu và <= phần còn lại (tất toán sớm ok)
            if (delta < minNeed || delta > remainingBefore)
                throw new InvalidOperationException(
                    $"Kỳ trả góp này phải ít nhất {minNeed:N0} VND và tối đa {remainingBefore:N0} VND.");

            contract.AmountPaid = newPaid;

            return await _contractRepository.UpdateContract(contract);
        }

        // --- Case 3: Đang ở bank_transfer/cash ---
        if (string.Equals(current, "bank_transfer", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(current, "cash", StringComparison.OrdinalIgnoreCase))
        {
            if (reqMethod == "installment")
                throw new InvalidOperationException("Không thể chuyển sang trả góp sau khi đã chọn hình thức thanh toán trực tiếp.");

            if (!string.Equals(current, reqMethod, StringComparison.OrdinalIgnoreCase))
                contract.PaymentMethod = reqMethod; // đổi cash <-> bank_transfer nếu khác

            if (newPaid < oldPaid)
                throw new InvalidOperationException("Số tiền mới không được nhỏ hơn số tiền hiện tại.");
            if (newPaid > total)
                throw new InvalidOperationException("Số tiền mới vượt quá tổng giá trị hợp đồng.");

            contract.AmountPaid = newPaid;

            return await _contractRepository.UpdateContract(contract);
        }

        throw new InvalidOperationException("Hợp đồng này đã hoàn tất và không thể thay đổi phương thức thanh toán.");
    }
}