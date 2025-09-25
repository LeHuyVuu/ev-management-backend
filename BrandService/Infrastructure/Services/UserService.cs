using AutoMapper;
using BrandService.DTOs.Requests.UserDTOs;
using BrandService.DTOs.Responses.UserDTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;

namespace BrandService.Infrastructure.Services;

public class UserService
{
    private readonly UserRepository _repo;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(UserRepository repo, ILogger<UserService> logger, IMapper mapper)
    {
        _repo = repo;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<UserResponse?> GetUserById(Guid id)
    {
        try
        {
            var entity = await _repo.GetUserById(id);
            if (entity == null) return null;

            return _mapper.Map<UserResponse>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting user by id: {Id}", id);
            return null;
        }
    }

    public async Task<UserResponse?> GetUserByEmail(string email)
    {
        try
        {
            var entity = await _repo.GetUserByEmail(email);
            if (entity == null) return null;

            return _mapper.Map<UserResponse>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<UserResponse?> CreateUser(UserRegisterRequest user)
    {
        try
        {
            var entity = _mapper.Map<User>(user);
            var created = await _repo.CreateUser(entity);

            return _mapper.Map<UserResponse>(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating user");
            return null;
        }
    }

    public async Task<UserResponse?> UpdateUser(UserUpdateRequest user)
    {
        try
        {
            // Kiểm tra tồn tại
            var exists = await _repo.CheckUserExists(user.UserId);
            if (!exists) return null;

            var existing = await _repo.GetUserById(user.UserId);
            if (existing == null) return null;

            _mapper.Map(user, existing); // map dữ liệu mới vào entity
            var updated = await _repo.UpdateUser(existing);

            return _mapper.Map<UserResponse>(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating user: {Id}", user.UserId);
            return null;
        }
    }

    public async Task<bool> UpdateStatusUser(Guid id, string status)
    {
        try
        {
            // Kiểm tra tồn tại
            var exists = await _repo.CheckUserExists(id);
            if (!exists) return false;

            var updated = await _repo.UpdateStatusUser(id, status);
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating status for user: {Id}", id);
            return false;
        }
    }


    public async Task<bool> CheckUserExists(Guid id)
    {
        return await _repo.CheckUserExists(id);
    }
}
