namespace JobPortalManagement.Models.DTO
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string RoleId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
