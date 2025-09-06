using JobPortalManagement.Models;
using System.Security.Claims;

namespace JobPortalManagement.Services.Interface
{
    public record AuthResult(string AccessToken, DateTime AccessTokenExpires, string RefreshToken, DateTime RefreshTokenExpires);

    public interface ITokenService
    {
        Task<string> GetValidAccessTokenAsync(string userName, HttpContext httpContext);
        Task<AuthResult> CreateTokensAsync(ApplicationUser user, string ipAddress);
        string CreateAccessToken(ApplicationUser user, DateTime expires);
        (string token, DateTime expires) CreateRefreshToken();
    }
}
