using Domain.Enums;
using System;

namespace Domain.Entities
{
    /// <summary>
    /// Persisted refresh token with user snapshot for rotation.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public string UserDisplayName { get; set; } = string.Empty;

        public UserRole UserRole { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }
    }
}
