using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using UnitTests.TestHelpers;
using Xunit;

namespace UnitTests.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public async Task GenerateTokensAsync_Persists_refresh_token_and_returns_tokens()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTests1");
        await using var context = new ApplicationDbContext(options);
        var service = new JwtTokenService(UnitTestHelper.CreateJwtSettingsOptions(), context, NullLogger<JwtTokenService>.Instance);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "tokenuser@example.com",
            DisplayName = "Token User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var tokens = await service.GenerateTokensAsync(user);

        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        Assert.True(tokens.ExpiresIn > 0);
        Assert.Equal(1, await context.RefreshTokens.CountAsync());
    }

    [Fact]
    public async Task ValidateTokenAsync_Returns_principal_for_valid_access_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTests2");
        await using var context = new ApplicationDbContext(options);
        var service = new JwtTokenService(UnitTestHelper.CreateJwtSettingsOptions(), context, NullLogger<JwtTokenService>.Instance);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "verify@example.com",
            DisplayName = "Verify User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var tokens = await service.GenerateTokensAsync(user);
        var principal = await service.ValidateTokenAsync(tokens.AccessToken);

        Assert.NotNull(principal);
        Assert.Equal(user.Email, principal!.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public async Task ValidateTokenAsync_Returns_null_for_invalid_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTestsInvalidToken");
        await using var context = new ApplicationDbContext(options);
        var service = new JwtTokenService(UnitTestHelper.CreateJwtSettingsOptions(), context, NullLogger<JwtTokenService>.Instance);

        var principal = await service.ValidateTokenAsync("invalid-token");

        Assert.Null(principal);
    }

    [Fact]
    public async Task RefreshTokensAsync_Returns_null_for_invalid_refresh_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTestsInvalidRefresh");
        await using var context = new ApplicationDbContext(options);
        var service = new JwtTokenService(UnitTestHelper.CreateJwtSettingsOptions(), context, NullLogger<JwtTokenService>.Instance);

        var refreshedTokens = await service.RefreshTokensAsync("invalid-refresh");

        Assert.Null(refreshedTokens);
    }

    [Fact]
    public async Task RefreshTokensAsync_Rotates_refresh_token_and_revokes_old_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTests3");
        await using var context = new ApplicationDbContext(options);
        var service = new JwtTokenService(UnitTestHelper.CreateJwtSettingsOptions(), context, NullLogger<JwtTokenService>.Instance);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "refresh@example.com",
            DisplayName = "Refresh User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var originalTokens = await service.GenerateTokensAsync(user);
        var refreshedTokens = await service.RefreshTokensAsync(originalTokens.RefreshToken);

        Assert.NotNull(refreshedTokens);
        Assert.NotEqual(originalTokens.RefreshToken, refreshedTokens!.RefreshToken);
        Assert.Equal(2, await context.RefreshTokens.CountAsync());

        var oldToken = await context.RefreshTokens.OrderBy(x => x.CreatedAt).FirstAsync();
        Assert.NotNull(oldToken.RevokedAt);
    }

    [Fact]
    public async Task RevokeTokenAsync_Sets_revoked_at_on_refresh_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTests4");
        await using var context = new ApplicationDbContext(options);
        var service = new JwtTokenService(UnitTestHelper.CreateJwtSettingsOptions(), context, NullLogger<JwtTokenService>.Instance);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "revoke@example.com",
            DisplayName = "Revoke User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var tokens = await service.GenerateTokensAsync(user);

        await service.RevokeTokenAsync(tokens.RefreshToken);

        var stored = await context.RefreshTokens.FirstOrDefaultAsync();
        Assert.NotNull(stored);
        Assert.NotNull(stored!.RevokedAt);
    }
}
