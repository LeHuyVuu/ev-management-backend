using System.Data;
using AutoMapper;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Extensions.Mapper;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

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

    public async Task<bool> CreateCustomer(CustomerCreateRequest request)
    {
        bool isExist = await _customerRepository.EmailExists(request.Email);
        if (isExist)
            throw new DuplicateNameException("Email already exists");
        var customer = _mapper.Map<CustomerCreateModel>(request);
        return await _customerRepository.CreateCustomer(_mapper.Map<Customer>(customer));
    }

    public async Task<bool> UpdateCustomer(CustomerUpdateRequest request)
    {
        var customer = await _customerRepository.GetCustomerById(request.CustomerId) ;
        if (customer == null)
            throw new KeyNotFoundException("Customer not found");
        var customerModel = _mapper.Map<CustomerUpdateModel>(request);
        _mapper.Map(customerModel, customer);
        if (request.DealerId.HasValue && request.DealerId.Value != Guid.Empty)
        {
            customer.DealerId = request.DealerId.Value;
        }
        return await _customerRepository.UpdateCustomer(customer);
    }
}