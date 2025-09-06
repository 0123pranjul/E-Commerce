using JobPortalManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System;
using JobPortalManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobPortalManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var fields = await _context.FormFields.OrderBy(f => f.DisplayOrder).ToListAsync();
            return View(fields);
        }

        [HttpPost]
        public async Task<IActionResult> AddField(string name, string type, bool isRequired, string options)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) return BadRequest("Name and type are required.");

            // Sanitize column name
            name = Regex.Replace(name, "[^a-zA-Z0-9_]", "");
            if (string.IsNullOrEmpty(name) || await IsReservedWord(name)) return BadRequest("Invalid or reserved column name.");

            // Validate options for dropdown and radio
            List<string> optionsList = null;
            if (type == "dropdown" || type == "radio")
            {
                if (string.IsNullOrEmpty(options)) return BadRequest("Options are required for dropdown and radio fields.");
                try
                {
                    optionsList = options.Split(',', StringSplitOptions.TrimEntries).ToList();
                    if (!optionsList.Any()) return BadRequest("At least one option is required.");
                }
                catch { return BadRequest("Invalid options format. Use comma-separated values."); }
            }

            // Map form type to SQL type
            string sqlType = type switch
            {
                "text" => "NVARCHAR(255)",
                "email" => "NVARCHAR(255)",
                "date" => "DATE",
                "checkbox" => "BIT", // Single checkbox stores true/false
                "dropdown" => "NVARCHAR(255)", // Store selected option as text
                "textarea" => "NVARCHAR(MAX)", // For multi-line text
                "radio" => "NVARCHAR(255)", // Store selected option as text
                _ => "NVARCHAR(255)"
            };

            // Check if column already exists
            var existingField = await _context.FormFields.FirstOrDefaultAsync(f => f.Name == name);
            if (existingField != null) return BadRequest("Field already exists.");

            try
            {
                // Add column to RegistrationDatas table
                await _context.Database.ExecuteSqlRawAsync(
                    $"ALTER TABLE RegistrationDatas ADD [{name}] {sqlType} {(isRequired ? "NOT NULL" : "NULL")}"
                );

                // Add to FormFields
                var maxOrder = await _context.FormFields.MaxAsync(f => (int?)f.DisplayOrder) ?? 0;
                var newField = new FormField
                {
                    Name = name,
                    Type = type,
                    IsRequired = isRequired,
                    DisplayOrder = maxOrder + 1,
                    Options = optionsList != null ? JsonSerializer.Serialize(optionsList) : null
                };
                _context.FormFields.Add(newField);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding column: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveField(int id)
        {
            var field = await _context.FormFields.FindAsync(id);
            if (field == null) return NotFound();

            try
            {
                // Drop column (WARNING: Deletes data!)
                await _context.Database.ExecuteSqlRawAsync($"ALTER TABLE RegistrationDatas DROP COLUMN [{field.Name}]");
                _context.FormFields.Remove(field);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing column: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        private async Task<bool> IsReservedWord(string name)
        {
            var reservedWords = new[] { "SELECT", "FROM", "WHERE", "ID", "SUBMITTEDAT" };
            return reservedWords.Contains(name.ToUpper()) || await _context.FormFields.AnyAsync(f => f.Name == name);
        }
    }
}
