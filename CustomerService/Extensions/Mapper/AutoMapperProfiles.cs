using AutoMapper;
using ProductService.DTOs;
using ProductService.DTOs.Requests.ContractDTOs;
using ProductService.DTOs.Requests.QuoteDTOs;
using ProductService.DTOs.Responses.QuoteDTOs;
using ProductService.Entities;

namespace ProductService.Extensions.Mapper
{
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
}
