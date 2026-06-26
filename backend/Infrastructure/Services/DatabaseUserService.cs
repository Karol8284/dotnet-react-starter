using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Infrastructure.Services;

public class DatabaseUserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseUserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return user is null
            ? ApiResponse<UserDto>.Error(404, "User not found")
            : ApiResponse<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        return user is null
            ? ApiResponse<UserDto>.Error(404, "User not found")
            : ApiResponse<UserDto>.Success(MapToDto(user));
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersPagedAsync(int pageNumber, int pageSize)
    {
        var safePageNumber = Math.Max(pageNumber, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return ApiResponse<List<UserDto>>.Success(users.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<int>> GetUserCountAsync()
    {
        var count = await _dbContext.Users.CountAsync();
        return ApiResponse<int>.Success(count);
    }

    public async Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return ApiResponse<bool>.Error(404, "User not found");
        }

        user.IsActive = false;
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "User deactivated");
    }

    public async Task<ApiResponse<bool>> ActivateUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return ApiResponse<bool>.Error(404, "User not found");
        }

        user.IsActive = true;
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "User activated");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return ApiResponse<bool>.Error(404, "User not found");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "User deleted");
    }

    public async Task<ApiResponse<UserDto>> UpdateDisplayNameAsync(Guid userId, string displayName)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return ApiResponse<UserDto>.Error(404, "User not found");
        }

        user.DisplayName = displayName.Trim();
        await _dbContext.SaveChangesAsync();

        return ApiResponse<UserDto>.Success(MapToDto(user), "Display name updated");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserRoleAsync(Guid userId, string role)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return ApiResponse<UserDto>.Error(404, "User not found");
        }

        if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
        {
            return ApiResponse<UserDto>.Error(400, "Invalid user role");
        }

        user.Role = parsedRole;
        await _dbContext.SaveChangesAsync();

        return ApiResponse<UserDto>.Success(MapToDto(user), "User role updated");
    }

    public async Task<ApiResponse<bool>> UserExistsAsync(Guid id)
    {
        var exists = await _dbContext.Users.AnyAsync(x => x.Id == id);
        return ApiResponse<bool>.Success(exists);
    }

    public async Task<ApiResponse<bool>> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        var normalizedEmail = NormalizeEmail(email);
        var exists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail && (!excludeUserId.HasValue || x.Id != excludeUserId.Value));
        return ApiResponse<bool>.Success(!exists);
    }

    public async Task<ApiResponse<bool>> IsUserActiveAsync(Guid userId)
    {
        var isActive = await _dbContext.Users.AnyAsync(x => x.Id == userId && x.IsActive);
        return ApiResponse<bool>.Success(isActive);
    }

    public async Task<ApiResponse<string>> GetUserRoleAsync(Guid userId)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        return user is null
            ? ApiResponse<string>.Error(404, "User not found")
            : ApiResponse<string>.Success(user.Role.ToString());
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static UserDto MapToDto(User user)
    {
        var displayNameParts = user.DisplayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new UserDto
        {
            Id = user.Id,
            FirstName = displayNameParts.Length > 0 ? displayNameParts[0] : string.Empty,
            LastName = displayNameParts.Length > 1 ? displayNameParts[1] : string.Empty,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
