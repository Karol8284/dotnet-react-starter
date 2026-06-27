using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)> PasswordResetTokens = new();
    private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)> EmailConfirmationTokens = new();

    public AuthService(ApplicationDbContext dbContext, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return passwordValid ? user : null;
    }

    public async Task<User?> RegisterAsync(string email, string password, string displayName)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (exists)
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = displayName.Trim(),
            Role = UserRole.User,
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public Task<bool> LogoutAsync(Guid userId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> UserExistsAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _dbContext.Users.AnyAsync(u => u.Email == normalizedEmail);
    }

    public Task<bool> IsEmailConfirmedAsync(Guid userId)
    {
        return _dbContext.Users.AnyAsync(u => u.Id == userId && u.IsEmailConfirmed);
    }

    public Task<bool> IsUserActiveAsync(Guid userId)
    {
        return _dbContext.Users.AnyAsync(u => u.Id == userId && u.IsActive);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user == null)
        {
            return null;
        }

        var token = Guid.NewGuid().ToString("N");
        PasswordResetTokens[token] = (user.Id, DateTime.UtcNow.AddHours(1));

        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user == null)
        {
            return false;
        }

        if (!PasswordResetTokens.TryGetValue(resetToken, out var tokenData))
        {
            return false;
        }

        if (tokenData.UserId != user.Id || tokenData.ExpiresAt < DateTime.UtcNow)
        {
            PasswordResetTokens.TryRemove(resetToken, out _);
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _dbContext.SaveChangesAsync();
        PasswordResetTokens.TryRemove(resetToken, out _);

        return true;
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        var userExists = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return null;
        }

        var token = Guid.NewGuid().ToString("N");
        EmailConfirmationTokens[token] = (userId, DateTime.UtcNow.AddDays(1));

        return token;
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string confirmationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        if (!EmailConfirmationTokens.TryGetValue(confirmationToken, out var tokenData))
        {
            return false;
        }

        if (tokenData.UserId != userId || tokenData.ExpiresAt < DateTime.UtcNow)
        {
            EmailConfirmationTokens.TryRemove(confirmationToken, out _);
            return false;
        }

        user.IsEmailConfirmed = true;
        await _dbContext.SaveChangesAsync();
        EmailConfirmationTokens.TryRemove(confirmationToken, out _);

        _logger.LogInformation("Email confirmed for user {UserId}", userId);
        return true;
    }
}
