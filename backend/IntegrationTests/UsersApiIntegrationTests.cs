using Domain.Entities;
using Application.DTOs.Auth;
using API.Controllers;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shared.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class UsersApiIntegrationTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersApiIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetAllUsers_Returns_unauthorized_when_token_is_missing()
    {
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_Returns_paged_users_when_authorized()
    {
        await SeedUserAsync("admin.users@example.com", "password123", "Admin Users", UserRole.Admin);
        await SeedUserAsync("user.one@example.com", "password123", "User One", UserRole.User);
        await SeedUserAsync("user.two@example.com", "password123", "User Two", UserRole.User);

        var tokens = await LoginAsync("admin.users@example.com", "password123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.GetAsync("/api/users?pageNumber=1&pageSize=2");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Application.DTOs.User.UserDto>>>();

        Assert.NotNull(result);
        Assert.NotNull(result!.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetUserById_Returns_user_when_authorized_and_user_exists()
    {
        var userId = await SeedUserAsync("lookup.user@example.com", "password123", "Lookup User", UserRole.User);
        await SeedUserAsync("admin.lookup@example.com", "password123", "Admin Lookup", UserRole.Admin);

        var tokens = await LoginAsync("admin.lookup@example.com", "password123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.GetAsync($"/api/users/{userId}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Application.DTOs.User.UserDto>>();

        Assert.NotNull(result);
        Assert.NotNull(result!.Data);
        Assert.Equal(userId, result.Data.Id);
        Assert.Equal("lookup.user@example.com", result.Data.Email);
    }

    [Fact]
    public async Task GetCount_Returns_forbidden_for_non_admin_user()
    {
        await SeedUserAsync("count.user@example.com", "password123", "Count User", UserRole.User);

        var tokens = await LoginAsync("count.user@example.com", "password123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.GetAsync("/api/users/count");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCount_Returns_user_count_for_admin()
    {
        await SeedUserAsync("count.admin@example.com", "password123", "Count Admin", UserRole.Admin);
        await SeedUserAsync("count.extra@example.com", "password123", "Count Extra", UserRole.User);

        var tokens = await LoginAsync("count.admin@example.com", "password123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.GetAsync("/api/users/count");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();

        Assert.NotNull(result);
        Assert.True(result!.Data >= 2);
    }

    [Fact]
    public async Task Delete_Removes_user_for_admin()
    {
        var userId = await SeedUserAsync("delete.user@example.com", "password123", "Delete User", UserRole.User);
        await SeedUserAsync("delete.admin@example.com", "password123", "Delete Admin", UserRole.Admin);

        var tokens = await LoginAsync("delete.admin@example.com", "password123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.DeleteAsync($"/api/users/{userId}");

        response.EnsureSuccessStatusCode();
        var deleteResult = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.NotNull(deleteResult);
        Assert.True(deleteResult!.Data);

        var getResponse = await _client.GetAsync($"/api/users/{userId}");
        getResponse.EnsureSuccessStatusCode();
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<Application.DTOs.User.UserDto>>();
        Assert.NotNull(getResult);
        Assert.Equal(404, getResult!.StatusCode);
        Assert.Null(getResult.Data);
    }

    [Fact]
    public async Task UpdateMe_Updates_profile_and_auth_me_returns_fresh_database_values()
    {
        await SeedUserAsync("profile.user@example.com", "password123", "Profile User", UserRole.User);

        var tokens = await LoginAsync("profile.user@example.com", "password123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var updateResponse = await _client.PutAsJsonAsync("/api/users/me", new
        {
            FirstName = "Updated",
            LastName = "Profile",
            Email = "updated.profile@example.com",
            AvatarUrl = "https://example.com/avatar.png"
        });

        updateResponse.EnsureSuccessStatusCode();

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<Application.DTOs.User.UserDto>>();
        Assert.NotNull(updateResult?.Data);
        Assert.Equal("Updated Profile", updateResult.Data.DisplayName);
        Assert.Equal("updated.profile@example.com", updateResult.Data.Email);
        Assert.Equal("https://example.com/avatar.png", updateResult.Data.AvatarUrl);

        var meResponse = await _client.GetAsync("/api/auth/me");

        meResponse.EnsureSuccessStatusCode();
        var meResult = await meResponse.Content.ReadFromJsonAsync<ApiResponse<CurrentUserDto>>();

        Assert.NotNull(meResult?.Data);
        Assert.Equal("Updated Profile", meResult.Data.DisplayName);
        Assert.Equal("Updated", meResult.Data.FirstName);
        Assert.Equal("Profile", meResult.Data.LastName);
        Assert.Equal("updated.profile@example.com", meResult.Data.Email);
        Assert.Equal("https://example.com/avatar.png", meResult.Data.AvatarUrl);
    }

    private async Task<AuthTokenResponse> LoginAsync(string email, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        var apiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>();
        Assert.NotNull(apiResponse?.Data);
        return apiResponse.Data;
    }

    private async Task<Guid> SeedUserAsync(string email, string password, string displayName, UserRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            Role = role,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user.Id;
    }

    private sealed class CurrentUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
