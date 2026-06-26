using Application.DTOs.User;
using Shared.Responses;

namespace Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email);
    Task<ApiResponse<List<UserDto>>> GetAllUsersPagedAsync(int pageNumber, int pageSize);
    Task<ApiResponse<int>> GetUserCountAsync();
    Task<ApiResponse<UserDto>> UpdateDisplayNameAsync(Guid userId, string displayName);
    Task<ApiResponse<UserDto>> UpdateUserRoleAsync(Guid userId, string role);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
    Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid userId);
    Task<ApiResponse<bool>> UserExistsAsync(Guid id);
    Task<ApiResponse<bool>> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);
    Task<ApiResponse<bool>> IsUserActiveAsync(Guid userId);
    Task<ApiResponse<string>> GetUserRoleAsync(Guid userId);
}
