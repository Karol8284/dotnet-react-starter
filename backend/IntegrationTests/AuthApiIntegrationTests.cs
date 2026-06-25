using Domain.Entities;
using Domain.Entities.JWT;
using Shared.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class AuthApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Login_Returns_tokens_for_valid_credentials()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "test@example.com", Password = "password123" });

        loginResponse.EnsureSuccessStatusCode();

        var apiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<JwtTokens>>();
        Assert.NotNull(apiResponse?.Data);
        Assert.False(string.IsNullOrWhiteSpace(apiResponse.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(apiResponse.Data.RefreshToken));
        Assert.True(apiResponse.Data.ExpiresIn > 0);
    }

    [Fact]
    public async Task Me_Returns_current_user_when_authorized()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "test@example.com", Password = "password123" });
        loginResponse.EnsureSuccessStatusCode();

        var loginApiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<JwtTokens>>();
        Assert.NotNull(loginApiResponse?.Data);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginApiResponse.Data.AccessToken);
        var meResponse = await _client.GetAsync("/api/auth/me");

        meResponse.EnsureSuccessStatusCode();
        var meApiResponse = await meResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(meApiResponse?.Data);
        Assert.Equal("Current user info", meApiResponse.Message);
    }

    [Fact]
    public async Task RefreshToken_Returns_new_tokens_when_refresh_token_is_valid()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "test@example.com", Password = "password123" });
        loginResponse.EnsureSuccessStatusCode();

        var loginApiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<JwtTokens>>();
        Assert.NotNull(loginApiResponse?.Data);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", new { RefreshToken = loginApiResponse.Data.RefreshToken });
        refreshResponse.EnsureSuccessStatusCode();

        var refreshApiResponse = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<JwtTokens>>();
        Assert.NotNull(refreshApiResponse?.Data);
        Assert.NotEqual(loginApiResponse.Data.RefreshToken, refreshApiResponse.Data.RefreshToken);
    }

    [Fact]
    public async Task Me_Returns_unauthorized_when_token_is_missing()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task Me_Returns_unauthorized_when_token_is_invalid()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task VerifyToken_Returns_unauthorized_when_token_is_invalid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/verify-token", new
        {
            Token = "invalid-token"
        });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Returns_unauthorized_when_refresh_token_is_invalid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            RefreshToken = "invalid-refresh-token"
        });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Cannot_reuse_old_refresh_token_after_rotation()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            Password = "password123"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginApiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<JwtTokens>>();
        Assert.NotNull(loginApiResponse?.Data);

        var oldRefreshToken = loginApiResponse.Data.RefreshToken;

        var firstRefreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            RefreshToken = oldRefreshToken
        });

        firstRefreshResponse.EnsureSuccessStatusCode();

        var secondRefreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            RefreshToken = oldRefreshToken
        });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, secondRefreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_Returns_unauthorized_when_access_token_is_missing()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/logout", new
        {
            RefreshToken = "any-refresh-token"
        });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

}
