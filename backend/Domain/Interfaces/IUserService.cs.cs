using Domain.Entities;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IUserService
    {
        // ========== READ OPERATIONS ==========

        /// <summary>
        /// Get user by ID.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>Result containing the user or error message</returns>
        Task<ApiResponse<User>> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Get user by email.
        /// </summary>
        /// <param name="email">The user email</param>
        /// <returns>Result containing the user or error message</returns>
        Task<ApiResponse<User>> GetUserByEmailAsync(string email);

        /// <summary>
        /// Get all users with pagination.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Result containing paginated users or error message</returns>
        Task<ApiResponse<List<User>>> GetAllUsersPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Get total count of users.
        /// </summary>
        /// <returns>Result containing user count or error message</returns>
        Task<ApiResponse<int>> GetUserCountAsync();

        // ========== WRITE OPERATIONS ==========

        /// <summary>
        /// Create a new user account.
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <returns>Result containing the created user or error message</returns>
        Task<ApiResponse<User>> CreateUserAsync(User user);

        /// <summary>
        /// Update user profile information.
        /// </summary>
        /// <param name="user">The user with updated values</param>
        /// <returns>Result containing the updated user or error message</returns>
        Task<ApiResponse<User>> UpdateUserAsync(User user);

        /// <summary>
        /// Deactivate user account.
        /// </summary>
        /// <param name="userId">The user ID to deactivate</param>
        /// <returns>Result indicating success or failure</returns>
        Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId);

        /// <summary>
        /// Activate user account.
        /// </summary>
        /// <param name="userId">The user ID to activate</param>
        /// <returns>Result indicating success or failure</returns>
        Task<ApiResponse<bool>> ActivateUserAsync(Guid userId);

        /// <summary>
        /// Delete user account.
        /// </summary>
        /// <param name="id">The user ID to delete</param>
        /// <returns>Result indicating success or failure</returns>
        Task<ApiResponse<bool>> DeleteUserAsync(Guid id);

        // ========== PROFILE OPERATIONS ==========

        /// <summary>
        /// Update user display name.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="displayName">The new display name</param>
        /// <returns>Result containing updated user or error message</returns>
        Task<ApiResponse<User>> UpdateDisplayNameAsync(Guid userId, string displayName);

        /// <summary>
        /// Update user role.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The new role</param>
        /// <returns>Result containing updated user or error message</returns>
        Task<ApiResponse<User>> UpdateUserRoleAsync(Guid userId, string role);

        // ========== VALIDATION OPERATIONS ==========

        /// <summary>
        /// Check if user exists.
        /// </summary>
        /// <param name="id">The user ID to check</param>
        /// <returns>Result indicating existence or error message</returns>
        Task<ApiResponse<bool>> UserExistsAsync(Guid id);

        /// <summary>
        /// Check if email is already in use.
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <param name="excludeUserId">Optional: Exclude specific user ID (for updates)</param>
        /// <returns>Result indicating if email is available or error message</returns>
        Task<ApiResponse<bool>> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);

        /// <summary>
        /// Check if user is active.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>Result indicating if user is active or error message</returns>
        Task<ApiResponse<bool>> IsUserActiveAsync(Guid userId);

        /// <summary>
        /// Get user role.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Result containing user role or error message</returns>
        Task<ApiResponse<string>> GetUserRoleAsync(Guid userId);
    }
}
