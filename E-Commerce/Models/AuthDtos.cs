using System.ComponentModel.DataAnnotations;

namespace JobPortalManagement.Models
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }


    public class LoginRequest
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = default!;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime AccessTokenExpires { get; set; }
        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenExpires { get; set; }
        public string UserId { get; set; } = default!;
        public string RoleId { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
