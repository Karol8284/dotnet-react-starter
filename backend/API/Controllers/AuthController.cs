using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Entities.JWT;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Shared.Responses;
using Shared.Settings;
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
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            IJwtTokenService jwtTokenService,
            IAuthService authService,
            ILogger<AuthController> logger,
            IOptions<JwtSettings> jwtOptions)
        {
            _jwtTokenService = jwtTokenService;
            _authService = authService;
            _logger = logger;
            _jwtSettings = jwtOptions.Value;
        }

        /// <summary>
        /// Login - Generate JWT tokens
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
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
                SetRefreshTokenCookie(tokens.RefreshToken);

                _logger.LogInformation("✓ Login successful for user: {UserId} ({Email})", user.Id, user.Email);

                return Ok(ApiResponse<AuthTokenResponse>.Success(CreateTokenResponse(tokens), "Login successful", 200));
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
        [EnableRateLimiting("AuthPolicy")]
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
                var displayName = $"{dto.FirstName} {dto.LastName}".Trim();
                var user = await _authService.RegisterAsync(dto.Email, dto.Password, displayName);
                if (user == null)
                {
                    _logger.LogError("❌ User registration failed for email: {Email}", dto.Email);
                    return StatusCode(500, ApiResponse<object>.Error(500, "User registration failed", null));
                }

                // Generate JWT tokens
                var tokens = await _jwtTokenService.GenerateTokensAsync(user);
                SetRefreshTokenCookie(tokens.RefreshToken);

                _logger.LogInformation("✓ Registration successful for user: {UserId} ({Email})", user.Id, user.Email);

                return Created($"api/auth/user/{user.Id}", ApiResponse<AuthTokenResponse>.Success(CreateTokenResponse(tokens), "Registration successful", 201));
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
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies[_jwtSettings.RefreshTokenCookieName];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest(ApiResponse<object>.Error(400, "Refresh token is required", null));

            try
            {
                _logger.LogInformation("🔄 Refresh token request");

                var tokens = await _jwtTokenService.RefreshTokensAsync(refreshToken);
                if (tokens == null)
                {
                    ClearRefreshTokenCookie();
                    return Unauthorized(ApiResponse<object>.Error(401, "Invalid or expired refresh token", null));
                }

                SetRefreshTokenCookie(tokens.RefreshToken);

                return Ok(ApiResponse<AuthTokenResponse>.Success(CreateTokenResponse(tokens), "Token refreshed successfully", 200));
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
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[_jwtSettings.RefreshTokenCookieName];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest(ApiResponse<object>.Error(400, "Refresh token is required", null));

            try
            {
                var userId = User.FindFirst("sub")?.Value;
                _logger.LogInformation("🚪 Logout request from user: {UserId}", userId);

                // Revoke refresh token
                await _jwtTokenService.RevokeTokenAsync(refreshToken);
                ClearRefreshTokenCookie();

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

        private AuthTokenResponse CreateTokenResponse(JwtTokens tokens)
        {
            return new AuthTokenResponse
            {
                AccessToken = tokens.AccessToken,
                ExpiresIn = tokens.ExpiresIn,
                TokenType = tokens.TokenType
            };
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append(_jwtSettings.RefreshTokenCookieName, refreshToken, CreateRefreshTokenCookieOptions());
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete(_jwtSettings.RefreshTokenCookieName, CreateRefreshTokenCookieOptions(DateTimeOffset.UnixEpoch));
        }

        private CookieOptions CreateRefreshTokenCookieOptions(DateTimeOffset? expires = null)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = _jwtSettings.RefreshTokenCookieIsEssential,
                Expires = expires ?? DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays),
                Path = _jwtSettings.RefreshTokenCookiePath,
                SameSite = ParseSameSiteMode(_jwtSettings.RefreshTokenCookieSameSite),
                Secure = ResolveSecureFlag(ParseSecurePolicy(_jwtSettings.RefreshTokenCookieSecurePolicy))
            };

            if (!string.IsNullOrWhiteSpace(_jwtSettings.RefreshTokenCookieDomain))
            {
                options.Domain = _jwtSettings.RefreshTokenCookieDomain;
            }

            return options;
        }

        private static SameSiteMode ParseSameSiteMode(string value)
        {
            return Enum.TryParse<SameSiteMode>(value, true, out var parsed)
                ? parsed
                : SameSiteMode.Lax;
        }

        private static CookieSecurePolicy ParseSecurePolicy(string value)
        {
            return Enum.TryParse<CookieSecurePolicy>(value, true, out var parsed)
                ? parsed
                : CookieSecurePolicy.SameAsRequest;
        }

        private bool ResolveSecureFlag(CookieSecurePolicy securePolicy)
        {
            return securePolicy switch
            {
                CookieSecurePolicy.Always => true,
                CookieSecurePolicy.None => false,
                _ => Request.IsHttps,
            };
        }
    }

    /// <summary>
    /// Public auth token response sent to the frontend.
    /// </summary>
    public class AuthTokenResponse
    {
        public required string AccessToken { get; set; }
        public required long ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
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
