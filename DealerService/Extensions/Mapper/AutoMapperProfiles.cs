using AutoMapper;
using ProductService.DTOs;
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
        }
    }
}
