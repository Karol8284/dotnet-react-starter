using Domain.Entities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuthService
    {
        // ========== AUTHENTICATION OPERATIONS ==========

        /// <summary>
        /// Authenticate user with email and password.
        /// Returns User entity if authentication successful.
        /// </summary>
        /// <param name="email">The user email</param>
        /// <param name="password">The user password</param>
        /// <returns>User entity or null if authentication failed</returns>
        Task<User?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Register a new user account.
        /// Creates user and returns the created user entity.
        /// </summary>
        /// <param name="email">The user email</param>
        /// <param name="password">The user password</param>
        /// <param name="displayName">The user display name</param>
        /// <returns>Created user entity or null if registration failed</returns>
        Task<User?> RegisterAsync(string email, string password, string displayName);

        /// <summary>
        /// Logout user (invalidate tokens).
        /// </summary>
        /// <param name="userId">The user ID to logout</param>
        /// <returns>Result indicating success or failure</returns>
        Task<bool> LogoutAsync(Guid userId);

        // ========== USER VERIFICATION ==========

        /// <summary>
        /// Verify if user email exists in system.
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <returns>True if user exists, false otherwise</returns>
        Task<bool> UserExistsAsync(string email);

        /// <summary>
        /// Verify if user email is confirmed.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if email is confirmed, false otherwise</returns>
        Task<bool> IsEmailConfirmedAsync(Guid userId);

        /// <summary>
        /// Check if user account is active.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if user is active, false otherwise</returns>
        Task<bool> IsUserActiveAsync(Guid userId);

        // ========== PASSWORD OPERATIONS ==========

        /// <summary>
        /// Change user password (requires current password verification).
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="currentPassword">The current password</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if password changed successfully, false otherwise</returns>
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// Reset user password (forgot password flow).
        /// Generates reset token.
        /// </summary>
        /// <param name="email">The user email</param>
        /// <returns>Reset token string or null if user not found</returns>
        Task<string?> GeneratePasswordResetTokenAsync(string email);

        /// <summary>
        /// Confirm password reset with token.
        /// </summary>
        /// <param name="email">The user email</param>
        /// <param name="resetToken">The reset token</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if password reset successfully, false otherwise</returns>
        Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword);

        // ========== EMAIL VERIFICATION ==========

        /// <summary>
        /// Generate email confirmation token.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Confirmation token string or null if user not found</returns>
        Task<string?> GenerateEmailConfirmationTokenAsync(Guid userId);

        /// <summary>
        /// Confirm user email with verification token.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="confirmationToken">The confirmation token</param>
        /// <returns>True if email confirmed successfully, false otherwise</returns>
        Task<bool> ConfirmEmailAsync(Guid userId, string confirmationToken);
    }
}
