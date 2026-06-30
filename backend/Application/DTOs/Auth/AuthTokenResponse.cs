using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// Public auth token response sent to the frontend.
    /// </summary>
    public class AuthTokenResponse
    {
        public required string AccessToken { get; set; }
        public required long ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
