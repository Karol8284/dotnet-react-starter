using Domain.Entities;
using System.Security.Claims;

namespace Domain.Interfaces
{
    /// <summary>
    /// JWT Token Service interface for token generation and validation
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate both access and refresh tokens for a user
        /// </summary>
        Task<JwtTokens> GenerateTokensAsync(User user);

        /// <summary>
        /// Validate and get claims from a JWT token
        /// </summary>
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);

        /// <summary>
        /// Revoke a refresh token (add to blacklist)
        /// </summary>
        Task RevokeTokenAsync(string refreshToken);

        /// <summary>
        /// Check if a refresh token is revoked
        /// </summary>
        Task<bool> IsTokenRevokedAsync(string refreshToken);
    }
}
