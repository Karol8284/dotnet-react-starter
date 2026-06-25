namespace Shared.Settings
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>Access token expiry. Default: 15 minutes.</summary>
        public int AccessTokenExpiresInMinutes { get; set; } = 15;

        /// <summary>Refresh token expiry. Default: 7 days.</summary>
        public int RefreshTokenExpiresInDays { get; set; } = 7;
    }
}
