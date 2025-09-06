using JobPortalManagement.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using JobPortalManagement.Models;

namespace JobPortalManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:44339/api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // ✅ AccessToken ko Session me store karo
            HttpContext.Session.SetString("AccessToken", result.AccessToken);
            // AccountController -> Login action ke andar (jab login success hota hai)
            HttpContext.Session.SetString("UserName", model.Username);
            HttpContext.Session.SetString("RoleId", result.RoleId);
            HttpContext.Session.SetString("UserId", result.UserId);

            // ✅ RefreshToken ko Secure HttpOnly cookie me store karo
            Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return RedirectToAction("DashBoard", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // ✅ RefreshToken nikaalo cookie se
            var refreshToken = Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync("https://localhost:44339/api/auth/logout", new
                {
                    RefreshToken = refreshToken
                });

                // Optional: check if API responded successfully
                if (!response.IsSuccessStatusCode)
                {
                    // Agar API error de to bhi hum local session clear karenge
                }
            }

            // ✅ Local session clear karo
            HttpContext.Session.Clear();

            // ✅ Cookie delete karo
            Response.Cookies.Delete("RefreshToken");

            return RedirectToAction("Index", "Home");
        }
    
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:44339/api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login", "Account");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration failed: {error}");
            return View(model);
        }


        public async Task<IActionResult> GetUserData()
        {
            var client = _httpClientFactory.CreateClient();

            var token = HttpContext.Session.GetString("AccessToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:5001/api/users");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // 🔄 Token expire ho gaya → refresh karna padega
                var newToken = await RefreshAccessToken();
                if (newToken == null) return RedirectToAction("Login", "Account");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await client.GetAsync("https://localhost:5001/api/users");
            }

            var data = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return View(data);
        }
        private async Task<string> RefreshAccessToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/auth/refresh", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            HttpContext.Session.SetString("AccessToken", result.AccessToken);

            return result.AccessToken;
        }


    }
}
