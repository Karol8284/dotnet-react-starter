using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class JwtTokenResponse
    {
        /// <summary>
        /// Access token (JWT) - used for API requests, expires in 15 minutes
        /// </summary>
        public required string AccessToken { get; set; }

        /// <summary>
        /// Refresh token - used to get new access token, expires in 7 days
        /// </summary>
        public required string RefreshToken { get; set; }

        /// <summary>
        /// Access token expiration time (Unix timestamp in seconds)
        /// </summary>
        public required long ExpiresIn { get; set; }

        /// <summary>
        /// Token type - always "Bearer"
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}
