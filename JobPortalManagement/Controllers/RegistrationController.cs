using JobPortalManagement.Data;
using JobPortalManagement.Models.DTO;
using JobPortalManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JobPortalManagement.Controllers
{
    [Route("Registration")]
   
    public class RegistrationController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        public RegistrationController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _db = db;
        }

        [HttpGet("Index")]
       
        public IActionResult Index()
        {
            var vm = new RegistrationPageVM
            {
                Fields = _db.FieldMasters
                            .Where(f => f.IsVisible)
                            .OrderBy(f => f.OrderNo)
                            .Select(f => new FieldMasterVM
                            {
                                FieldId = f.FieldId,
                                FieldName = f.FieldName,
                                Label = f.Label,
                                FieldType = f.FieldType,
                                IsRequired = f.IsRequired,
                                IsVisible = f.IsVisible,
                                OrderNo = f.OrderNo,
                                OptionsJson = f.OptionsJson
                            }).ToList()
            };

            return View(vm);
        }

        // POST: /register
        [HttpPost("Index")]
        [ValidateAntiForgeryToken]
        
        public IActionResult Index(RegistrationPageVM model)
        {
            if (!ModelState.IsValid)
            {
                // reload fields if validation fails
                model.Fields = _db.FieldMasters
                                  .Where(f => f.IsVisible)
                                  .OrderBy(f => f.OrderNo)
                                  .Select(f => new FieldMasterVM
                                  {
                                      FieldId = f.FieldId,
                                      FieldName = f.FieldName,
                                      Label = f.Label,
                                      FieldType = f.FieldType,
                                      IsRequired = f.IsRequired,
                                      IsVisible = f.IsVisible,
                                      OrderNo = f.OrderNo,
                                      OptionsJson = f.OptionsJson
                                  }).ToList();
                return View(model);
            }

            // Capture posted values
            var form = Request.Form;

            // fixed fields
            string username = form["Username"];
            string password = form["Password"];

            // capture dynamic fields into dictionary
            var dynamicData = new Dictionary<string, string>();
            foreach (var field in _db.FieldMasters.Where(f => f.IsVisible))
            {
                var value = form[field.FieldName].ToString();
                dynamicData[field.FieldName] = value;
            }

            // Save into DB
            var entity = new UserRegistration
            {
                Username = username,
                PasswordHash = password, // ⚠️ in real apps, hash this!
                DynamicData = JsonConvert.SerializeObject(dynamicData),
                CreatedAt = DateTime.UtcNow
            };

            _db.UserRegistrations.Add(entity);
            _db.SaveChanges();

            TempData["Success"] = "Registration successful!";
            return RedirectToAction("Index");
        }
    }
}
