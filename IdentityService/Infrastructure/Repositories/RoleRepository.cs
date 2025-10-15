using IdentityService.Context;
using IdentityService.DTOs.Responses.RoleDTOs;
using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class RoleRepository
{
    private readonly MyDbContext _context;

    public RoleRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAll()
    {
        return await _context.Roles
            .OrderBy(r => r.RoleId)
            .ToListAsync();
    }

}