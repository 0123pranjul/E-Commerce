namespace JobPortalManagement.Models.DTO
{
    public class UserDto
    {
        public int UserId { get; set; }          // PK User table se
        public string Username { get; set; }     // Login ke liye
        public string Name { get; set; }         // Full name (First + Last)
        public string Email { get; set; }        // Email id
        public string MobileNo { get; set; }     // Contact number
        public string Address { get; set; }      // Address
        public string Role { get; set; }         // Role (Admin/User/etc.)
        public string Token { get; set; }        // JWT Access Token
        public string RefreshToken { get; set; } // Refresh token
        public DateTime TokenExpiry { get; set; } // JWT Expiry
    }
   
}
