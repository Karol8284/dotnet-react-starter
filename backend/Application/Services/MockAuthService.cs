using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Mock Authentication Service - for testing JWT without database
    /// Replace with real implementation later
    /// </summary>
    public class MockAuthService : IAuthService
    {
        private readonly ILogger<MockAuthService> _logger;

        // Hardcoded test users
        private static readonly User TestUser = new()
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = Domain.Enums.UserRole.User,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = "hashed_password_123" // In real app, use BCrypt
        };

        public MockAuthService(ILogger<MockAuthService> logger)
        {
            _logger = logger;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            _logger.LogInformation("🔐 Mock authentication attempt: {Email}", email);

            // Mock: accept any password for test@example.com
            if (email == TestUser.Email && password == "password123")
            {
                _logger.LogInformation("✓ Mock authentication successful");
                return await Task.FromResult(TestUser);
            }

            _logger.LogWarning("⚠️ Mock authentication failed for {Email}", email);
            return await Task.FromResult<User?>(null);
        }

        public async Task<User?> RegisterAsync(string email, string password, string displayName)
        {
            _logger.LogInformation("📝 Mock registration: {Email}", email);

            // Mock: always accept registration
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName,
                Role = Domain.Enums.UserRole.User,
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = "hashed_password_" + Guid.NewGuid().ToString().Substring(0, 8)
            };

            _logger.LogInformation("✓ Mock registration successful for user {UserId}", newUser.Id);
            return await Task.FromResult(newUser);
        }

        public async Task<bool> LogoutAsync(Guid userId)
        {
            _logger.LogInformation("🚪 Mock logout for user {UserId}", userId);
            return await Task.FromResult(true);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await Task.FromResult(email == TestUser.Email);
        }

        public async Task<bool> IsEmailConfirmedAsync(Guid userId)
        {
            return await Task.FromResult(userId == TestUser.Id);
        }

        public async Task<bool> IsUserActiveAsync(Guid userId)
        {
            return await Task.FromResult(userId == TestUser.Id);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            _logger.LogInformation("🔑 Mock change password for user {UserId}", userId);
            return await Task.FromResult(true);
        }

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            return await Task.FromResult(Guid.NewGuid().ToString());
        }

        public async Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword)
        {
            _logger.LogInformation("🔄 Mock password reset for {Email}", email);
            return await Task.FromResult(true);
        }

        public async Task<string?> GenerateEmailConfirmationTokenAsync(Guid userId)
        {
            return await Task.FromResult(Guid.NewGuid().ToString());
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string confirmationToken)
        {
            _logger.LogInformation("✉️ Mock email confirmation for user {UserId}", userId);
            return await Task.FromResult(true);
        }
    }
}
