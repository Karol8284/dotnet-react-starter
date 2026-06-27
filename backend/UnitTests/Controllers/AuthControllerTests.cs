using API.Controllers;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Entities.JWT;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Responses;
using Shared.Settings;
using System.Security.Claims;
using UnitTests.TestHelpers;
using Xunit;

namespace UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<ILogger<AuthController>> _loggerMock = new();
    private readonly IOptions<JwtSettings> _jwtOptions = UnitTestHelper.CreateJwtSettingsOptions();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_jwtTokenServiceMock.Object, _authServiceMock.Object, _loggerMock.Object, _jwtOptions);
    }

    [Fact]
    public async Task Login_Returns_ok_when_credentials_are_valid()
    {
        var dto = new LoginUserDto { Email = "test@example.com", Password = "password123" };
        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, DisplayName = "Test", Role = UserRole.User };
        var tokens = new JwtTokens { AccessToken = "access-token", RefreshToken = "refresh-token", ExpiresIn = 900 };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _authServiceMock.Setup(x => x.AuthenticateAsync(dto.Email, dto.Password)).ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(x => x.GenerateTokensAsync(user)).ReturnsAsync(tokens);

        var actionResult = await _controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<AuthTokenResponse>>(okResult.Value);
        var setCookieHeader = _controller.Response.Headers.SetCookie.ToString();

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal("Login successful", response.Message);
        Assert.Equal(tokens.AccessToken, response.Data?.AccessToken);
        Assert.Contains("drs.refreshToken=refresh-token", setCookieHeader);
        Assert.Contains("httponly", setCookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", setCookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_Returns_unauthorized_when_authentication_fails()
    {
        var dto = new LoginUserDto { Email = "bad@example.com", Password = "wrong" };
        _authServiceMock.Setup(x => x.AuthenticateAsync(dto.Email, dto.Password)).ReturnsAsync((User?)null);

        var actionResult = await _controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(actionResult);
    }

    [Fact]
    public async Task Register_Returns_created_when_registration_succeeds()
    {
        var dto = new RegisterUserDto { Email = "new@example.com", Password = "password123", FirstName = "New", LastName = "User", CreatedAt = DateTime.UtcNow };
        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, DisplayName = "New User", Role = UserRole.User };
        var tokens = new JwtTokens { AccessToken = "access-token", RefreshToken = "refresh-token", ExpiresIn = 900 };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _authServiceMock.Setup(x => x.UserExistsAsync(dto.Email)).ReturnsAsync(false);
        _authServiceMock.Setup(x => x.RegisterAsync(dto.Email, dto.Password, "New User")).ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(x => x.GenerateTokensAsync(user)).ReturnsAsync(tokens);

        var actionResult = await _controller.Register(dto);

        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        var response = Assert.IsType<ApiResponse<AuthTokenResponse>>(createdResult.Value);
        var setCookieHeader = _controller.Response.Headers.SetCookie.ToString();

        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("Registration successful", response.Message);
        Assert.Equal(tokens.AccessToken, response.Data?.AccessToken);
        Assert.Contains("drs.refreshToken=refresh-token", setCookieHeader);
        Assert.Contains("httponly", setCookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_Returns_bad_request_when_user_already_exists()
    {
        var dto = new RegisterUserDto { Email = "test@example.com", Password = "password123", FirstName = "New", LastName = "User", CreatedAt = DateTime.UtcNow };

        _authServiceMock.Setup(x => x.UserExistsAsync(dto.Email)).ReturnsAsync(true);

        var actionResult = await _controller.Register(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);

        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal("User with this email already exists", response.Message);
    }

    [Fact]
    public async Task Logout_Returns_bad_request_when_refresh_token_is_missing()
    {
        var userId = Guid.NewGuid().ToString();
        var user = ControllerTestHelper.CreateAuthenticatedUser(userId, "user@test.com");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = ControllerTestHelper.CreateHttpContext(user)
        };

        var actionResult = await _controller.Logout();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);

        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal("Refresh token is required", response.Message);
    }

    [Fact]
    public async Task VerifyToken_Returns_bad_request_when_token_is_missing()
    {
        var request = new VerifyTokenRequest { Token = string.Empty };

        var actionResult = await _controller.VerifyToken(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);

        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal("Token is required", response.Message);
    }

    [Fact]
    public async Task RefreshToken_Returns_ok_when_refresh_succeeds()
    {
        var tokens = new JwtTokens { AccessToken = "access-token", RefreshToken = "new-refresh-token", ExpiresIn = 900 };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = "drs.refreshToken=refresh-token";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _jwtTokenServiceMock.Setup(x => x.RefreshTokensAsync("refresh-token")).ReturnsAsync(tokens);

        var actionResult = await _controller.RefreshToken();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<AuthTokenResponse>>(okResult.Value);
        var setCookieHeader = _controller.Response.Headers.SetCookie.ToString();

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(tokens.AccessToken, response.Data?.AccessToken);
        Assert.Contains("drs.refreshToken=new-refresh-token", setCookieHeader);
        Assert.Contains("httponly", setCookieHeader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshToken_Returns_unauthorized_when_refresh_token_is_invalid()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = "drs.refreshToken=invalid-refresh";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _jwtTokenServiceMock.Setup(x => x.RefreshTokensAsync("invalid-refresh")).ReturnsAsync((JwtTokens?)null);

        var actionResult = await _controller.RefreshToken();
        var setCookieHeader = _controller.Response.Headers.SetCookie.ToString();

        Assert.IsType<UnauthorizedObjectResult>(actionResult);
        Assert.Contains("drs.refreshToken=", setCookieHeader);
        Assert.Contains("expires=thu, 01 jan 1970", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_Returns_ok_when_refresh_token_is_revoked()
    {
        var userId = Guid.NewGuid().ToString();
        var user = ControllerTestHelper.CreateAuthenticatedUser(userId, "user@test.com");
        var httpContext = ControllerTestHelper.CreateHttpContext(user);
        httpContext.Request.Headers.Cookie = "drs.refreshToken=refresh-token";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _jwtTokenServiceMock.Setup(x => x.RevokeTokenAsync("refresh-token")).Returns(Task.CompletedTask);

        var actionResult = await _controller.Logout();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        var setCookieHeader = _controller.Response.Headers.SetCookie.ToString();

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal("Logout successful", response.Message);
        Assert.Contains("drs.refreshToken=", setCookieHeader);
        Assert.Contains("expires=thu, 01 jan 1970", setCookieHeader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyToken_Returns_ok_when_token_is_valid()
    {
        var request = new VerifyTokenRequest { Token = "valid-token" };

        _jwtTokenServiceMock.Setup(x => x.ValidateTokenAsync(request.Token)).ReturnsAsync(ControllerTestHelper.CreateAuthenticatedUser(Guid.NewGuid().ToString(), "verify@test.com"));

        var actionResult = await _controller.VerifyToken(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal("Token is valid", response.Message);
    }

    [Fact]
    public void GetCurrentUser_Returns_ok_when_user_is_authenticated()
    {
        var user = ControllerTestHelper.CreateAuthenticatedUser(Guid.NewGuid().ToString(), "me@example.com");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = ControllerTestHelper.CreateHttpContext(user)
        };

        var actionResult = _controller.GetCurrentUser();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal("Current user info", response.Message);
    }
}
