using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
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
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementation of JWT Token Service
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private const int AccessTokenExpirationMinutes = 15;
        private const int RefreshTokenExpirationDays = 7;

        public JwtTokenService(
            IOptions<JwtSettings> jwtOptions,
            ApplicationDbContext dbContext,
            ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _dbContext = dbContext;
            _logger = logger;
        }
        /// <summary>
        /// Generate JWT access token and refresh token
        /// </summary>
        public async Task<JwtTokens> GenerateTokensAsync(User user)
        {
            try
            {
                var accessTokenExpiration = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);
                var accessTokenString = CreateAccessToken(user.Id, user.Email, user.DisplayName, user.Role, user.IsEmailConfirmed, accessTokenExpiration);

                // Generate Refresh Token (long-lived, random string)
                var refreshToken = GenerateRefreshToken();
                var refreshTokenHash = HashToken(refreshToken);

                _dbContext.RefreshTokens.Add(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserDisplayName = user.DisplayName,
                    UserRole = user.Role,
                    IsEmailConfirmed = user.IsEmailConfirmed,
                    TokenHash = refreshTokenHash,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)
                });

                await _dbContext.SaveChangesAsync();

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

        public async Task<JwtTokens?> RefreshTokensAsync(string refreshToken)
        {
            try
            {
                var tokenHash = HashToken(refreshToken);
                var storedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

                if (storedToken == null)
                {
                    _logger.LogWarning("⚠️ Refresh token not found");
                    return null;
                }

                if (storedToken.RevokedAt.HasValue || storedToken.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning("⚠️ Refresh token is expired or revoked");
                    return null;
                }

                var newTokens = await GenerateTokensAsync(new User
                {
                    Id = storedToken.UserId,
                    Email = storedToken.UserEmail,
                    DisplayName = storedToken.UserDisplayName,
                    Role = storedToken.UserRole,
                    IsEmailConfirmed = storedToken.IsEmailConfirmed
                });

                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.ReplacedByTokenHash = HashToken(newTokens.RefreshToken);
                await _dbContext.SaveChangesAsync();

                return newTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error refreshing tokens");
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
            var tokenHash = HashToken(refreshToken);
            var storedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (storedToken != null && !storedToken.RevokedAt.HasValue)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("🔐 Refresh token revoked");
        }

        /// <summary>
        /// Check if refresh token is in revoked list
        /// </summary>
        public async Task<bool> IsTokenRevokedAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);
            var storedToken = await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            return storedToken == null || storedToken.RevokedAt.HasValue || storedToken.ExpiresAt <= DateTime.UtcNow;
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

        private string CreateAccessToken(Guid userId, string email, string displayName, Domain.Enums.UserRole role, bool isEmailConfirmed, DateTime expiresAt)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, displayName),
                new(ClaimTypes.Role, role.ToString()),
                new("IsEmailConfirmed", isEmailConfirmed.ToString()),
            };

            var accessTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.CreateToken(accessTokenDescriptor);
            return tokenHandler.WriteToken(accessToken);
        }

        private static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}