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
    private readonly IMapper _mapper;

    public CustomerService(CustomerRepository customerRepository,  IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper =  mapper;
    }

    public async Task<IEnumerable<CustomerBasicResponse>> GetAllCustomers()
    {
        var customers = await _customerRepository.GetAllCustomers();
        return _mapper.Map<IEnumerable<CustomerBasicResponse>>(customers);
    }

    public async Task<CustomerDetailResponse> GetCustomerDetail(Guid customerId)
    {
        var customer = await _customerRepository.GetCustomerDetail(customerId);
        if(customer is null)
            throw new KeyNotFoundException("Customer not found");
        return _mapper.Map<CustomerDetailResponse>(customer);
    }

    public async Task<bool> CreateCustomer(Guid dealerId, CustomerCreateRequest request)
    {
        bool isExist = await _customerRepository.EmailExists(request.Email);
        if (isExist)
            throw new DuplicateNameException("Email already exists");

        var customer = _mapper.Map<Customer>(request);
        customer.DealerId = dealerId;
        return await _customerRepository.CreateCustomer(customer);
    }

    public async Task<bool> UpdateCustomer(CustomerUpdateRequest request)
    {
        var customer = await _customerRepository.GetCustomerById(request.CustomerId) ;
        
        if (customer == null)
            throw new KeyNotFoundException("Customer not found");

        if (request.DealerId == null || request.DealerId == Guid.Empty)
        {
            request.DealerId = customer.DealerId;
        }
        
        var customerModel = _mapper.Map<CustomerUpdateModel>(request);
        _mapper.Map(customerModel, customer);
        
        return await _customerRepository.UpdateCustomer(customer);
    }
}