using AutoMapper;
using ProductService.DTOs;
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
}