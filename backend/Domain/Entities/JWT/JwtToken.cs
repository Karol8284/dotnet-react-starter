using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.JWT
{
    /// <summary>
    /// DTO returned by GenerateTokensAsync method.
    /// Short lived access token (JWT) and long-lived refresh token.
    /// Contains both access and refresh tokens along with expiration info.
    /// NOT persisted in database - only transferred between layers.
    /// </summary>
    public class JwtTokens
    {
        /// <summary>
        /// Access token (JWT) - used for API requests, expires in 15 minutes.
        /// </summary>
        public required string AccessToken { get; set; }

        /// <summary>
        /// Refresh token - raw string used to get new access token, expires in 7 days.
        /// This is the RAW value (not hashed). The hash is stored in RefreshToken entity.
        /// </summary>
        public required string RefreshToken { get; set; }

        /// <summary>
        /// Access token expiration time in seconds from now.
        /// </summary>
        public required long ExpiresIn { get; set; }

        /// <summary>
        /// Token type - always "Bearer" for JWT.
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}
