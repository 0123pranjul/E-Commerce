using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using JobPortalManagement.Models;
using JobPortalManagement.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using JobPortalManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using JobPortalManagement.Services.Interface;
using JobPortalManagement.Services;

namespace JobPortalManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager, ApplicationDbContext db, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DashBoard()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var accessToken = await _tokenService.GetValidAccessTokenAsync(userName, HttpContext);

            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://localhost:44339/api/Profile/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Token invalid ya expired hai.";
                return RedirectToAction("Login", "Account");
            }

            var userJson = await response.Content.ReadAsStringAsync();
            var userDto = JsonConvert.DeserializeObject<UserDto>(userJson);
            return View(userDto);
        }


       
        public IActionResult About()
        {
            return View();
        }
         public IActionResult Blog()
        {
            return View();
        }
         public IActionResult Contect()
        {
            return View();
        }
           public IActionResult schedule()
        {
            return View();
        }
           public IActionResult speakers()
        {
            return View();
        }
             public IActionResult blogsingle()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
