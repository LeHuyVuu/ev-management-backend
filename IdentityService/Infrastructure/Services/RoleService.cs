using AutoMapper;
using IdentityService.DTOs.Responses.RoleDTOs;
using IdentityService.Infrastructure.Repositories;

namespace IdentityService.Infrastructure.Services;

public class RoleService
{
    private readonly RoleRepository _repository;
    private readonly IMapper _mapper;
    public RoleService(RoleRepository repository,IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<RoleResponse>> GetAllRoles()
    {
        var roles = await _repository.GetAll();
        return  _mapper.Map<List<RoleResponse>>(roles); ;
    }
}