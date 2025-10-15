using System.Data;
using AutoMapper;
using CustomerService.DTOs.Requests.CustomerDTOs;
using CustomerService.DTOs.Responses.CustomerDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;
using CustomerService.Models;

namespace CustomerService.Infrastructure.Services;

public class CustomerService
{
    private readonly CustomerRepository _customerRepository;
    private readonly DealerRepository _dealerRepository;
    private readonly UserRepository _userRepository;
    private readonly IMapper _mapper;

    public CustomerService(
        CustomerRepository customerRepository,  
        DealerRepository dealerRepository,
        UserRepository userRepository,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _dealerRepository = dealerRepository;
        _customerRepository =  customerRepository;
        _userRepository = userRepository;
        _mapper =  mapper;
    }

    public async Task<IEnumerable<CustomerBasicResponse>> GetCustomersByDealerId(Guid dealerId)
    {
        var customers = await _customerRepository.GetCustomersByDealerId(dealerId);
        if (customers == null || !customers.Any())
            return Enumerable.Empty<CustomerBasicResponse>();

        var customerResponses = _mapper.Map<IEnumerable<CustomerBasicResponse>>(customers) 
                                ?? Enumerable.Empty<CustomerBasicResponse>();

        foreach (var customer in customerResponses)
        {
            var user = await _userRepository.GetUserStaffByDealerId(dealerId);
            customer.StaffContact = user?.Name ?? "No assigned staff";
        }

        return customerResponses;
    }

    public async Task<CustomerDetailResponse> GetCustomerDetail(Guid customerId)
    {
        var customer = await _customerRepository.GetCustomerDetail(customerId);
        if (customer == null)
            throw new KeyNotFoundException("Customer not found");

        return _mapper.Map<CustomerDetailResponse>(customer);
    }

    public async Task<bool> CreateCustomer(Guid dealerId, CustomerCreateRequest request)
    {
        var emailExists = await _customerRepository.EmailExists(request.Email);
        if (emailExists)
            throw new DuplicateNameException("Email already exists");

        var customerModel = _mapper.Map<CustomerCreateModel>(request);
        customerModel.DealerId = dealerId;

        var customerEntity = _mapper.Map<Customer>(customerModel);
        return await _customerRepository.CreateCustomer(customerEntity);
    }

    public async Task<bool> UpdateCustomer(CustomerUpdateRequest request)
    {
        var customer = await _customerRepository.GetCustomerById(request.CustomerId);
        if (customer == null)
            throw new KeyNotFoundException("Customer not found");

        var updatedModel = _mapper.Map<CustomerUpdateModel>(request);
        updatedModel.DealerId = customer.DealerId;

        _mapper.Map(updatedModel, customer);
        return await _customerRepository.UpdateCustomer(customer);
    }

}