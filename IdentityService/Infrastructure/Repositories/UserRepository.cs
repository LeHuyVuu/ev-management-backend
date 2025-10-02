using IdentityService.Context;
using IdentityService.Entities;
using IdentityService.Extensions.Query;
using IdentityService.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository
{
    private readonly MyDbContext _context;

    public UserRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await _context.Users
                .AsNoTracking()
                .OrderByDescending(d => d.UserId)
                .ToPagedResultAsync(pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }
    
    public async Task<User?> GetUserById(Guid id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email); 
    }

    public async Task<User?> CreateUser(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUser(User user)
    {

        _context.Users.Update(user);   
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateStatusUser(Guid id, string status)
    {
        var user = await GetUserById(id);
        user.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckUserExists(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.UserId == id);
    }

    public async Task<bool> UpdateRole(Guid id, int role)
    {
        var user = await GetUserById(id);
        user.RoleId = role;
        await _context.SaveChangesAsync();
        return true;
    }
}