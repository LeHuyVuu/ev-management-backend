using AutoMapper;
using CustomerService.DTOs.Requests.OrderDTOs;
using CustomerService.DTOs.Responses.OrderDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;
using CustomerService.Models;

namespace CustomerService.Infrastructure.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly ContractRepository _contractRepository;
    private readonly DealerRepository _dealerRepository;
    private readonly QuoteRepository _quoteRepository;
    private readonly InventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public OrderService(
        OrderRepository orderRepository, 
        CustomerRepository customerRepository,
        ContractRepository contractRepository,
        DealerRepository dealerRepository,
        QuoteRepository quoteRepository,
        InventoryRepository inventoryRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _contractRepository = contractRepository;
        _dealerRepository = dealerRepository;
        _quoteRepository = quoteRepository;
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderCustomerResponse>> GetAllOrdersByCustomerId(Guid customerId)
    {
        var customerExists = await _customerRepository.CustomerExists(customerId);
        if (!customerExists)
            throw new KeyNotFoundException("Customer does not exist");

        var orders = await _orderRepository.GetAllOrdersByCustomerId(customerId);
        if (orders == null || !orders.Any())
            return Enumerable.Empty<OrderCustomerResponse>();

        var orderResponses = _mapper.Map<IEnumerable<OrderCustomerResponse>>(orders) 
                             ?? Enumerable.Empty<OrderCustomerResponse>();

        return orderResponses;
    }

    public async Task<bool> CreateOrder(Guid dealerId, OrderCreateRequest request)
    {
        if(request.ContractId == Guid.Empty)
            throw new KeyNotFoundException("Please input contract Id");
        if(request.CustomerId == Guid.Empty)
            throw new KeyNotFoundException("Please input customer Id");
        
        var customer = await _customerRepository.GetCustomerById(request.CustomerId);
        if(customer == null)
            throw new KeyNotFoundException("Customer does not exist");
        var contract = await _contractRepository.GetContractByContractIncludeQuote(request.ContractId);
        if(contract == null)
            throw new KeyNotFoundException("Contract does not exist");
        
        if(customer.CustomerId != contract.CustomerId)
            throw new Exception("The customer of the order does not match the customer in the contract.");
        
        if(request.DeliveryAddress == null)
            throw new KeyNotFoundException("Delivery Address is not null");
        if(request.DeliveryDate == null)
            throw new KeyNotFoundException("Delivery Date is not null");
        
        var order = _mapper.Map<OrderCreateModel>(request);
        order.DealerId = dealerId;
        
        var inventory = await _inventoryRepository.GetInventoryByVersionId(contract.Quote.VehicleVersionId);
        if(inventory == null)
            throw new KeyNotFoundException("Car in inventory does not exist");
        
        int quantity = inventory.StockQuantity;

        if (quantity <= 0)
            throw new Exception("The vehicle version is out of stock and cannot place an order.");
        inventory.StockQuantity = quantity - 1;
        await _inventoryRepository.UpdateInventory(inventory);

        if(contract.Status != "approved"){
            contract.Status = "approved";
            await _contractRepository.UpdateContractStatus(contract);
        }

        var result = await _orderRepository.CreateOrder(_mapper.Map<Order>(order));
        if (!result)
        {
            throw new Exception("Failed to create order");
        }
        return result;
    }

    public async Task<OrderDetailResponse> GetOrderDetailByOrderId(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByOrderId(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");
        
        var customer = await _customerRepository.GetCustomerById(order.CustomerId);
        if (customer == null)
            throw new KeyNotFoundException("Customer does not exist");


        var quote = await _quoteRepository.GetQuoteByContractId(order.ContractId);
        if (quote == null)
            throw new KeyNotFoundException("Quote not found");
        
        var vehicle = await _quoteRepository.GetVehicleByQuoteId(quote.QuoteId);
        if (vehicle == null)
            throw new KeyNotFoundException("Vehicle not found");

        var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quote.QuoteId);
        if (vehicleVersion == null)
            throw new KeyNotFoundException("Vehicle not found");

        return new OrderDetailResponse()
        {
            OrderId = order.OrderId,
            Name = customer.Name,
            Phone = customer.Phone ?? "Unknown",
            Email = customer.Email ?? "Unknown",
            Brand = vehicle.Brand,
            ModelName = vehicle.ModelName,
            VersionName = vehicleVersion.VersionName,
            Color = vehicleVersion.Color ?? "Unknown",
            DeliveryAddress = order.DeliveryAddress ?? "Unknown",
            DeliveryDate = order.DeliveryDate,
            Status = order.Status,
        };
    }

    public async Task<IEnumerable<OrderDetailResponse>> GetOrdersByDealerId(Guid dealerId)
    {
        if (dealerId == Guid.Empty)
            throw new KeyNotFoundException("Dealer does not exist");
        var dealer = await _dealerRepository.GetDealerByDealerId(dealerId);
        if (dealer == null)
            throw new KeyNotFoundException("Dealer not found");

        var orders = await _orderRepository.GetOrdersByDealerId(dealerId);
        
        return orders.Select(o => new OrderDetailResponse
        {
            OrderId = o.OrderId,
            Name = o.Contract?.Customer?.Name ?? "Unknown",
            Phone = o.Contract?.Customer?.Phone ?? "Unknown",
            Email = o.Contract?.Customer?.Email ?? "Unknown",
            Brand = o.Contract?.Quote?.VehicleVersion?.Vehicle?.Brand ?? "Unknown",
            ModelName = o.Contract?.Quote?.VehicleVersion?.Vehicle?.ModelName ?? "Unknown",
            VersionName = o.Contract?.Quote?.VehicleVersion?.VersionName ?? "Unknown",
            Color = o.Contract?.Quote?.VehicleVersion?.Color ?? "Unknown",
            DeliveryAddress = o.DeliveryAddress ?? "Unknown",
            DeliveryDate = o.DeliveryDate,
            Status = o.Status
        });
    }

    public async Task<bool> UpdateOrderStatus(Guid orderId, OrderUpdateStatusRequest request)
    {
        if (orderId == Guid.Empty)
            throw new KeyNotFoundException("Order Id is not null");
        if(request.Status == null)
            throw new KeyNotFoundException("Status update is not null");

        var order = await _orderRepository.GetOrderByOrderId(orderId);
        if(order == null)
            throw new KeyNotFoundException("Order does not exist");
        
        var allowedStatuses = new[] { "preparing", "shipped from warehouse", "in transit", "arrived", "out for delivery", "completed", "delivery failed", "waiting" };
        var status = string.IsNullOrWhiteSpace(request.Status) ? "draft" : request.Status.Trim().ToLowerInvariant();
        if (!allowedStatuses.Contains(status))
            throw new InvalidOperationException("Invalid status value. Only accepted: preparing, shipped from warehouse, in transit, arrived, out for delivery, completed, delivery failed, waiting");


        if (string.Equals(request.Status, "delivery failed", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(order.Status, "delivery failed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("You cannot update an order that has failed delivery.");
        }

        if (string.Equals(request.Status, "delivery failed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(order.Status, "delivery failed", StringComparison.OrdinalIgnoreCase))
        {
            var contract = await _contractRepository.GetContractByContractIncludeQuote(order.ContractId);
            if (contract == null)
                throw new KeyNotFoundException("Contract does not exist");
            var inventory = await _inventoryRepository.GetInventoryByVersionId(contract.Quote.VehicleVersionId);
            if (inventory == null)
                throw new KeyNotFoundException("Car in inventory does not exist");

            int quantity = inventory.StockQuantity;

            inventory.StockQuantity = quantity + 1;
            await _inventoryRepository.UpdateInventory(inventory);

            contract.Status = "draft";
            await _contractRepository.UpdateContractStatus(contract);
        }

        order.Status = request.Status;

        return await _orderRepository.UpdateOrderStatus(order);
    }

    public async Task<bool> UpdateOrderLimitedAsync(Guid orderId, OrderUpdateRequest request)
    {
        var order = await _orderRepository.GetOrderByOrderId(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");

        if (!string.Equals(order.Status, "preparing", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only orders in 'preparing' status can be edited.");

        if (request.DeliveryAddress != null)
            order.DeliveryAddress = request.DeliveryAddress;
        if (request.DeliveryDate != null)
            order.DeliveryDate = request.DeliveryDate;
        // Note: not persisted unless Order entity has a Note column. You can extend entity later.

        return await _orderRepository.UpdateOrderLimitedAsync(order);
    }
}