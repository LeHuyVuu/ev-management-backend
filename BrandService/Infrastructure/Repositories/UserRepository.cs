using BrandService.Context;
using BrandService.DTOs;
using BrandService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories;

public class UserRepository
{
    private readonly MyDbContext _context;

    public UserRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email); 
    }

    public async Task<User?> CreateUser(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUser(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;

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
}