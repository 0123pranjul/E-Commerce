using JobPortalManagement.Data;
using JobPortalManagement.Models;
using JobPortalManagement.Models.DTO;
using JobPortalManagement.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JobPortalManagement.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _cfg;
        private readonly ApplicationDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        public TokenService(IConfiguration cfg, ApplicationDbContext db, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager)
        {
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _cfg = cfg;
            _db = db;
        }

        public async Task<AuthResult> CreateTokensAsync(ApplicationUser user, string ipAddress)
        {
            var accessExpires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:AccessTokenMinutes"]!));
            var refreshExpires = DateTime.UtcNow.AddDays(int.Parse(_cfg["Jwt:RefreshTokenDays"]!));

            var accessToken = CreateAccessToken(user, accessExpires);
            var (refreshToken, _) = CreateRefreshToken();

            var rt = new RefreshToken
            {
                Token = refreshToken,
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                Expires = refreshExpires,
                UserId = user.Id
            };

            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync();

            return new AuthResult(accessToken, accessExpires, refreshToken, refreshExpires);
        }

        public string CreateAccessToken(ApplicationUser user, DateTime expires)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? "")
            };

            // roles
            // NOTE: If you need roles in token, load and add them here.

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string token, DateTime expires) CreateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(bytes);
            var expires = DateTime.UtcNow.AddDays(int.Parse(_cfg["Jwt:RefreshTokenDays"]!));
            return (token, expires);
        }
        public async Task<string> GetValidAccessTokenAsync(string userName, HttpContext httpContext)
        {
            var accessToken = httpContext.Session.GetString("AccessToken");
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || string.IsNullOrEmpty(accessToken))
                return null;

            var refreshToken = await _db.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
                .OrderByDescending(rt => rt.Created)
                .FirstOrDefaultAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://localhost:44339/api/Profile/me");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && refreshToken != null)
            {
                // Refresh token
                var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44339/api/Auth/refresh")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken.Token
                    }), Encoding.UTF8, "application/json")
                };

                var refreshResponse = await client.SendAsync(refreshRequest);
                if (refreshResponse.IsSuccessStatusCode)
                {
                    var tokens = JsonConvert.DeserializeObject<TokenResponse>(
                        await refreshResponse.Content.ReadAsStringAsync());

                    // Update session
                    httpContext.Session.SetString("AccessToken", tokens.AccessToken);

                    // Revoke old token
                    refreshToken.Revoked = DateTime.UtcNow;
                    refreshToken.ReasonRevoked = "Replaced by new token";
                    refreshToken.ReplacedByToken = tokens.RefreshToken;

                    // Generate unique new refresh token
                    string newToken = GenerateRefreshToken();
                    while (await _db.RefreshTokens.AnyAsync(t => t.Token == newToken))
                    {
                        newToken = GenerateRefreshToken();
                    }

                    var newRefreshToken = new RefreshToken
                    {
                        Token = newToken,
                        Created = DateTime.UtcNow,
                        CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString(),
                        Expires = DateTime.UtcNow.AddDays(7),
                        UserId = user.Id
                    };

                    _db.RefreshTokens.Add(newRefreshToken);
                    await _db.SaveChangesAsync();

                    return tokens.AccessToken;
                }
            }

            return accessToken; // Existing token is still valid
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
