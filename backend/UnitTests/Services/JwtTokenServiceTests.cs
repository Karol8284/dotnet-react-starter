using Domain.Entities;
using Domain.Entities.JWT;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using UnitTests.TestHelpers;
using Xunit;

namespace UnitTests.Services;

public class JwtTokenServiceTests
{
    // Helper: tworzy serwis z mock IHttpContextAccessor (brak kontekstu HTTP w testach)
    private static JwtTokenService CreateService(ApplicationDbContext context)
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        return new JwtTokenService(
            UnitTestHelper.CreateJwtSettingsOptions(),
            context,
            NullLogger<JwtTokenService>.Instance,
            httpContextAccessor.Object);
    }

    [Fact]
    public async Task GenerateTokensAsync_Persists_refresh_token_and_returns_tokens()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTests1");
        await using var context = new ApplicationDbContext(options);
        var service = CreateService(context);

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
        var service = CreateService(context);

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
        var service = CreateService(context);

        var principal = await service.ValidateTokenAsync("invalid-token");

        Assert.Null(principal);
    }

    [Fact]
    public async Task RefreshTokensAsync_Returns_null_for_invalid_refresh_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTestsInvalidRefresh");
        await using var context = new ApplicationDbContext(options);
        var service = CreateService(context);

        var refreshedTokens = await service.RefreshTokensAsync("invalid-refresh");

        Assert.Null(refreshedTokens);
    }

    [Fact]
    public async Task RefreshTokensAsync_Rotates_refresh_token_and_revokes_old_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTests3");
        await using var context = new ApplicationDbContext(options);
        var service = CreateService(context);

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
        var service = CreateService(context);

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

    [Fact]
    public async Task IsTokenRevokedAsync_Returns_false_for_active_refresh_token()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTestsActiveRefresh");
        await using var context = new ApplicationDbContext(options);
        var service = CreateService(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "active-refresh@example.com",
            DisplayName = "Active Refresh User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var tokens = await service.GenerateTokensAsync(user);

        var isRevoked = await service.IsTokenRevokedAsync(tokens.RefreshToken);

        Assert.False(isRevoked);
    }

    [Fact]
    public async Task IsTokenRevokedAsync_Returns_true_after_refresh_token_is_revoked()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTestsRevokedRefresh");
        await using var context = new ApplicationDbContext(options);
        var service = CreateService(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "revoked-refresh@example.com",
            DisplayName = "Revoked Refresh User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var tokens = await service.GenerateTokensAsync(user);

        await service.RevokeTokenAsync(tokens.RefreshToken);

        var isRevoked = await service.IsTokenRevokedAsync(tokens.RefreshToken);

        Assert.True(isRevoked);
    }

    [Fact]
    public async Task RefreshTokensAsync_Returns_null_when_refresh_token_was_already_rotated()
    {
        var options = UnitTestHelper.CreateInMemoryDatabaseOptions("JwtTokenServiceTestsRefreshReuse");
        await using var context = new ApplicationDbContext(options);
        var service = CreateService(context);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "reuse-refresh@example.com",
            DisplayName = "Reuse Refresh User",
            Role = UserRole.User,
            IsEmailConfirmed = true
        };

        var originalTokens = await service.GenerateTokensAsync(user);

        var firstRefresh = await service.RefreshTokensAsync(originalTokens.RefreshToken);
        var secondRefresh = await service.RefreshTokensAsync(originalTokens.RefreshToken);

        Assert.NotNull(firstRefresh);
        Assert.Null(secondRefresh);
    }
}
