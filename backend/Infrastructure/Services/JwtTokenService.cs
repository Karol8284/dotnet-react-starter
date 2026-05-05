using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementation of JWT Token Service
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;
        private static readonly HashSet<string> RevokedTokens = new();
        private const int AccessTokenExpirationMinutes = 15;
        private const int RefreshTokenExpirationDays = 7;

        public JwtTokenService(IOptions<JwtSettings> jwtOptions, ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger;
        }
        /// <summary>
        /// Generate JWT access token and refresh token
        /// </summary>
        public async Task<JwtTokens> GenerateTokensAsync(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create claims for user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.DisplayName),
                    new(ClaimTypes.Role, user.Role.ToString()),
                    new("IsEmailConfirmed", user.IsEmailConfirmed.ToString()),
                };

                // Generate Access Token (short-lived)
                var accessTokenExpiration = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);
                var accessTokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = accessTokenExpiration,
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    SigningCredentials = credentials,
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var accessToken = tokenHandler.CreateToken(accessTokenDescriptor);
                var accessTokenString = tokenHandler.WriteToken(accessToken);

                // Generate Refresh Token (long-lived, random string)
                var refreshToken = GenerateRefreshToken();

                _logger.LogInformation(
                    "✓ Generated tokens for user {UserId} ({Email}). Access token expires at {ExpiresAt}",
                    user.Id,
                    user.Email,
                    accessTokenExpiration
                );

                return await Task.FromResult(new JwtTokens
                {
                    AccessToken = accessTokenString,
                    RefreshToken = refreshToken,
                    ExpiresIn = (long)(accessTokenExpiration - DateTime.UtcNow).TotalSeconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating tokens for user {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Validate JWT token and return claims
        /// </summary>
        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return await Task.FromResult(principal);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Token validation failed");
                return null;
            }
        }

        /// <summary>
        /// Revoke refresh token by adding to blacklist
        /// </summary>
        public async Task RevokeTokenAsync(string refreshToken)
        {
            RevokedTokens.Add(refreshToken);
            _logger.LogInformation("🔐 Refresh token revoked");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Check if refresh token is in revoked list
        /// </summary>
        public async Task<bool> IsTokenRevokedAsync(string refreshToken)
        {
            return await Task.FromResult(RevokedTokens.Contains(refreshToken));
        }

        /// <summary>
        /// Generate random refresh token (256-bit)
        /// </summary>
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}