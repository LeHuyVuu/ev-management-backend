using AutoMapper;
using CustomerService.DTOs.Requests.ContractDTOs;
using CustomerService.DTOs.Requests.CustomerDTOs;
using CustomerService.DTOs.Requests.QuoteDTOs;
using CustomerService.DTOs.Responses.ContractDTOs;
using CustomerService.DTOs.Responses.CustomerDTOs;
using CustomerService.DTOs.Responses.OrderDTOs;
using CustomerService.Entities;
using CustomerService.Models;

namespace CustomerService.Extensions.Mapper;

public class AutoMapperProfiles : Profile
{
        public AutoMapperProfiles()
        {
            //Customer
            CreateMap<Customer, CustomerBasicResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            CreateMap<Customer, CustomerDetailResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            CreateMap<CustomerCreateRequest, CustomerCreateModel>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            CreateMap<CustomerCreateModel, Customer>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            CreateMap<CustomerUpdateRequest, CustomerUpdateModel>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            CreateMap<CustomerUpdateModel, Customer>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            
            // Order
            CreateMap<Order, OrderCustomerResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            
            // Contract
            CreateMap<Contract, ContractCustomerResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            
            CreateMap<ContractCreateRequest, Contract>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            
            CreateMap<Contract, ContractDetailResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
            
            CreateMap<Contract, ContractDealerResponse>()
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));

            // Quote
            CreateMap<QuoteUpdateRequest, Quote>()
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForAllMembers(opt 
                    => opt.Condition((src, dest, srcMember) 
                        => srcMember != null));
        }
}