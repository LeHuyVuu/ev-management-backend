using AutoMapper;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Extensions.Mapper;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

public class CustomerService
{
    private readonly CustomerRepository _CustomerRepository;
    private readonly IMapper _mapper;

    public CustomerService(CustomerRepository customerRepository,  IMapper mapper)
    {
        _CustomerRepository = customerRepository;
        _mapper =  mapper;
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllCustomers()
    {
        var customers = await _CustomerRepository.GetAllCustomers();
        return _mapper.Map<IEnumerable<CustomerResponse>>(customers);
    }

    public async Task<CustomerDetailResponse> GetCustomerDetail(Guid customerId)
    {
        var customer = await _CustomerRepository.GetCustomerDetail(customerId);
        return _mapper.Map<CustomerDetailResponse>(customer);
    }

    public async Task<bool> CreateCustomer(CustomerCreateRequest request)
    {
        var customer = _mapper.Map<CustomerCreateModel>(request);
        return await _CustomerRepository.CreateCustomer(_mapper.Map<Customer>(customer));
    }

    public async Task<bool> UpdateCustomer(CustomerUpdateRequest request)
    {
        var customer = _mapper.Map<CustomerUpdateModel>(request);
        return await _CustomerRepository.UpdateCustomer(_mapper.Map<Customer>(customer));
    }
}