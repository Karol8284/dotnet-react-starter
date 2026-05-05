using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Responses;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IJwtTokenService jwtTokenService,
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _jwtTokenService = jwtTokenService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login - Generate JWT tokens
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error(400, "Invalid login data", null));

            try
            {
                _logger.LogInformation("🔐 Login attempt for email: {Email}", dto.Email);

                // Authenticate user (verify email and password)
                var user = await _authService.AuthenticateAsync(dto.Email, dto.Password);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ Login failed for email: {Email}", dto.Email);
                    return Unauthorized(ApiResponse<object>.Error(401, "Invalid email or password", null));
                }

                // Generate JWT tokens
                var tokens = await _jwtTokenService.GenerateTokensAsync(user);

                _logger.LogInformation("✓ Login successful for user: {UserId} ({Email})", user.Id, user.Email);

                return Ok(ApiResponse<JwtTokens>.Success(tokens, "Login successful", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Login error");
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error", null));
            }
        }

        /// <summary>
        /// Register - Create new user account
        /// POST /api/auth/register
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error(400, "Invalid registration data", null));

            try
            {
                _logger.LogInformation("📝 Registration attempt for email: {Email}", dto.Email);

                // Check if user already exists
                var userExists = await _authService.UserExistsAsync(dto.Email);
                if (userExists)
                {
                    _logger.LogWarning("⚠️ Registration failed: User already exists with email: {Email}", dto.Email);
                    return BadRequest(ApiResponse<object>.Error(400, "User with this email already exists", null));
                }

                // Register user
                var user = await _authService.RegisterAsync(dto.Email, "password", dto.FirstName);
                if (user == null)
                {
                    _logger.LogError("❌ User registration failed for email: {Email}", dto.Email);
                    return StatusCode(500, ApiResponse<object>.Error(500, "User registration failed", null));
                }

                // Generate JWT tokens
                var tokens = await _jwtTokenService.GenerateTokensAsync(user);

                _logger.LogInformation("✓ Registration successful for user: {UserId} ({Email})", user.Id, user.Email);

                return Created($"api/auth/user/{user.Id}", ApiResponse<JwtTokens>.Success(tokens, "Registration successful", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Registration error");
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error", null));
            }
        }

        /// <summary>
        /// Refresh Token - Get new access token using refresh token
        /// POST /api/auth/refresh-token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                return BadRequest(ApiResponse<object>.Error(400, "Refresh token is required", null));

            try
            {
                _logger.LogInformation("🔄 Refresh token request");

                // Check if refresh token is revoked
                var isRevoked = await _jwtTokenService.IsTokenRevokedAsync(request.RefreshToken);
                if (isRevoked)
                {
                    _logger.LogWarning("⚠️ Refresh token is revoked");
                    return Unauthorized(ApiResponse<object>.Error(401, "Refresh token has been revoked", null));
                }

                // TODO: Verify refresh token in database and get associated user
                // For now, returning error as this requires DB integration
                _logger.LogError("❌ Refresh token verification not implemented");
                return Unauthorized(ApiResponse<object>.Error(401, "Invalid refresh token", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Refresh token error");
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error", null));
            }
        }

        /// <summary>
        /// Logout - Revoke refresh token
        /// POST /api/auth/logout
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                return BadRequest(ApiResponse<object>.Error(400, "Refresh token is required", null));

            try
            {
                var userId = User.FindFirst("sub")?.Value;
                _logger.LogInformation("🚪 Logout request from user: {UserId}", userId);

                // Revoke refresh token
                await _jwtTokenService.RevokeTokenAsync(request.RefreshToken);

                _logger.LogInformation("✓ Logout successful for user: {UserId}", userId);

                return Ok(new ApiResponse(200, "Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Logout error");
                return StatusCode(500, ApiResponse.Error(500, "Internal server error", null));
            }
        }

        /// <summary>
        /// Get current user info (protected endpoint)
        /// GET /api/auth/me
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                var email = User.FindFirst("email")?.Value;
                var displayName = User.FindFirst("unique_name")?.Value;
                var role = User.FindFirst("role")?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(ApiResponse<object>.Error(401, "User not authenticated", null));

                var userData = new
                {
                    id = userId,
                    email = email,
                    displayName = displayName,
                    role = role
                };

                return Ok(ApiResponse<object>.Success(userData, "Current user info", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving current user");
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error", null));
            }
        }

        /// <summary>
        /// Verify token - Check if token is valid
        /// POST /api/auth/verify-token
        /// </summary>
        [HttpPost("verify-token")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
                return BadRequest(ApiResponse<object>.Error(400, "Token is required", null));

            try
            {
                _logger.LogInformation("🔍 Token verification request");

                var principal = await _jwtTokenService.ValidateTokenAsync(request.Token);
                if (principal == null)
                {
                    _logger.LogWarning("⚠️ Token validation failed");
                    return Unauthorized(ApiResponse<object>.Error(401, "Invalid token", null));
                }

                _logger.LogInformation("✓ Token is valid");

                return Ok(ApiResponse<object>.Success(new { isValid = true }, "Token is valid", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Token verification error");
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error", null));
            }
        }
    }

    /// <summary>
    /// Request DTO for refresh token endpoint
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for logout endpoint
    /// </summary>
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for token verification
    /// </summary>
    public class VerifyTokenRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
