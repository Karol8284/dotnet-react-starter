using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DatabaseAuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseAuthService> _logger;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public DatabaseAuthService(ApplicationDbContext dbContext, ILogger<DatabaseAuthService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Authentication failed for {Email}", normalizedEmail);
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Authentication failed for {Email}", normalizedEmail);
            return null;
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _dbContext.SaveChangesAsync();
        }

        return user;
    }

    public async Task<User?> RegisterAsync(string email, string password, string displayName)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail))
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = displayName.Trim(),
            Role = UserRole.User,
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Registered user {UserId} ({Email})", user.Id, user.Email);
        return user;
    }

    public Task<bool> LogoutAsync(Guid userId)
        => Task.FromResult(true);

    public async Task<bool> UserExistsAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        return await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
    }

    public async Task<bool> IsEmailConfirmedAsync(Guid userId)
        => await _dbContext.Users.AnyAsync(x => x.Id == userId && x.IsEmailConfirmed);

    public async Task<bool> IsUserActiveAsync(Guid userId)
        => await _dbContext.Users.AnyAsync(x => x.Id == userId && x.IsActive);

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return false;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        return await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
    }

    public Task<string?> GeneratePasswordResetTokenAsync(string email)
        => Task.FromResult<string?>(null);

    public Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword)
        => Task.FromResult(false);

    public Task<string?> GenerateEmailConfirmationTokenAsync(Guid userId)
        => Task.FromResult<string?>(null);

    public async Task<bool> ConfirmEmailAsync(Guid userId, string confirmationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return false;
        }

        user.IsEmailConfirmed = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmEmailConfirmedAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        return await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail && x.IsEmailConfirmed);
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();
}
