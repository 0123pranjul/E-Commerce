using JobPortalManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace JobPortalManagement.Controllers
{
    public class RegistrationAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationAdminController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var fields = await _context.FormFields.OrderBy(f => f.DisplayOrder).ToListAsync();
            ViewBag.Fields = fields;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(IFormCollection form)
        {
            var fields = await _context.FormFields.ToListAsync();
            if (!fields.Any()) return BadRequest("No fields defined.");

            var columns = new List<string> { "SubmittedAt" };
            var values = new List<string> { $"'{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}'" };

            foreach (var field in fields)
            {
                string value = field.Type == "checkbox" ? string.Join(",", form[field.Name]) : form[field.Name].ToString();

                if (field.IsRequired && string.IsNullOrEmpty(value))
                    return BadRequest($"Missing required field: {field.Name}");

                if (field.Type == "checkbox")
                    value = string.IsNullOrEmpty(value) ? "0" : "1"; // Convert to BIT (0 or 1)
                else if (field.Type == "date" && !string.IsNullOrEmpty(value))
                    value = DateTime.Parse(value).ToString("yyyy-MM-dd");

                columns.Add($"[{field.Name}]");
                values.Add(field.Type == "checkbox" ? value : $"'{value.Replace("'", "''")}'");
            }

            try
            {
                var sql = $"INSERT INTO RegistrationDatas ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error saving data: {ex.Message}");
            }

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
