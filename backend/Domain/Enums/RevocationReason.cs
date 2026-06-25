namespace Domain.Enums
{
    /// <summary>
    /// Reasons for refresh token revocation.
    /// </summary>
    public enum RevocationReason
    {
        /// <summary>
        /// User manually logged out.
        /// </summary>
        UserLogout = 0,

        /// <summary>
        /// Token expired naturally (reached ExpiresAt).
        /// </summary>
        TokenExpired = 1,

        /// <summary>
        /// Administrator revoked the token.
        /// </summary>
        AdminRevoke = 2,

        /// <summary>
        /// Security event triggered revocation (e.g., suspicious activity, password change).
        /// </summary>
        SecurityEvent = 3,

        /// <summary>
        /// Token revoked due to device change or new device login.
        /// </summary>
        DeviceChange = 4,

        /// <summary>
        /// Token was rotated - replaced by a new token during refresh.
        /// </summary>
        TokenRotated = 5
    }
}
