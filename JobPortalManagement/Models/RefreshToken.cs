namespace JobPortalManagement.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = default!;
        public DateTime Created { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime Expires { get; set; }

        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;

        // FK
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
    }
}
