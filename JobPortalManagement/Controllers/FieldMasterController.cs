using JobPortalManagement.Data;
using JobPortalManagement.Models;
using JobPortalManagement.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace JobPortalManagement.Controllers
{
    [Route("FieldMaster")]
    public class FieldMasterController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        public FieldMasterController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _db = db;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var vm = new FieldMasterListVM
            {
                Fields = _db.FieldMasters
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
                CssClass= f.CssClass,
                OptionsJson = f.OptionsJson
            }).ToList()
            };
            return View(vm);
        }


        [HttpPost("save")]
        [ValidateAntiForgeryToken]
     
        public IActionResult Save(FieldMasterListVM vm)
        {
            if (vm?.Fields == null) return RedirectToAction(nameof(Index));


            var ids = vm.Fields.Select(x => x.FieldId).ToList();
            var entities = _db.FieldMasters.Where(f => ids.Contains(f.FieldId)).ToList();


            foreach (var item in vm.Fields)
            {
                var e = entities.First(x => x.FieldId == item.FieldId);
                e.Label = item.Label;
                e.FieldType = item.FieldType;
                e.IsRequired = item.IsRequired;
                e.IsVisible = item.IsVisible;
                e.OrderNo = item.OrderNo;
                e.OptionsJson = item.OptionsJson;
            }
            _db.SaveChanges();
            TempData["msg"] = "Fields updated";
            return RedirectToAction(nameof(Index));
        }
    }
}
