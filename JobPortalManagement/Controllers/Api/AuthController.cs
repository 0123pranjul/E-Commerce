using JobPortalManagement.Data;
using JobPortalManagement.Models;
using JobPortalManagement.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;

namespace JobPortalManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly ITokenService _tokens;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, ITokenService tokens, ApplicationDbContext db, RoleManager<IdentityRole> roleManager)
        {
            _users = users; _signIn = signIn; _tokens = tokens; _db = db; _roleManager = roleManager;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (req.Password != req.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = new ApplicationUser
            {
                UserName = req.UserName,
                Email = req.Email
            };

            var result = await _users.CreateAsync(user, req.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "User created successfully" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
        {
            var dd= await _users.Users.ToListAsync();
            var user = await _users.Users.FirstOrDefaultAsync(u => u.UserName == req.UserName);
            if (user == null) return Unauthorized();

            var pw = await _signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
            if (!pw.Succeeded) return Unauthorized();

            var roles = await _users.GetRolesAsync(user);
            var roleId = roles.Any()
                ? (await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roles.First()))?.Id
                : null;


            var auth = await _tokens.CreateTokensAsync(user, GetIp());
            return Ok(new AuthResponse
            {
                AccessToken = auth.AccessToken,
                AccessTokenExpires = auth.AccessTokenExpires,
                RefreshToken = auth.RefreshToken,
                RefreshTokenExpires = auth.RefreshTokenExpires,
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                RoleId = roleId
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Session clear
            HttpContext.Session.Clear();

            // RefreshToken delete
            Response.Cookies.Delete("RefreshToken");

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req)
        {
            var rt = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == req.RefreshToken);

            if (rt == null || !rt.IsActive)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            // revoke only THIS token (not all devices)
            rt.Revoked = DateTime.UtcNow;
            rt.RevokedByIp = GetIp();

            // generate new refresh token safely
            var replacement = await GenerateUniqueRefreshToken(rt.UserId);
            rt.ReplacedByToken = replacement.Token;

            // persist changes
            _db.RefreshTokens.Add(replacement);
            await _db.SaveChangesAsync();

            // create new access token
            var accessExpires = DateTime.UtcNow.AddMinutes(
                int.Parse(HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Jwt:AccessTokenMinutes"]!));

            var access = _tokens.CreateAccessToken(rt.User, accessExpires);

            return Ok(new AuthResponse
            {
                AccessToken = access,
                AccessTokenExpires = accessExpires,
                RefreshToken = replacement.Token,
                RefreshTokenExpires = replacement.Expires,
                UserId = rt.User.Id,
                UserName = rt.User.UserName!,
                Email = rt.User.Email!
            });
        }

        /// <summary>
        /// Generate refresh token that is guaranteed unique (loops until DB check passes).
        /// </summary>
        private async Task<RefreshToken> GenerateUniqueRefreshToken(string userId)
        {
            string token;
            bool exists;

            do
            {
                token = GenerateSecureToken();
                exists = await _db.RefreshTokens.AnyAsync(r => r.Token == token);
            }
            while (exists);

            return new RefreshToken
            {
                Token = token,
                Created = DateTime.UtcNow, // ✅ keep UtcNow
                CreatedByIp = GetIp(),
                Expires = DateTime.UtcNow.AddDays(int.Parse(
                    HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:RefreshTokenDays"]!
                )),
                UserId = userId
            };
        }


        /// <summary>
        /// Generates a cryptographically secure token.
        /// </summary>
        private string GenerateSecureToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }



        [HttpPost("revoke")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequest req)
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
            if (rt == null || !rt.IsActive) return NotFound();

            rt.Revoked = DateTime.Now;
            rt.RevokedByIp = GetIp();
            rt.ReasonRevoked = "Manual revoke";
            await _db.SaveChangesAsync();

            return Ok(new { message = "revoked" });
        }

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
