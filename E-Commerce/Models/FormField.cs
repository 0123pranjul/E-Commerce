using System.Text.Json;

namespace JobPortalManagement.Models
{
    public class FormField
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "FirstName"
        public string Type { get; set; } // e.g., "text", "email", "date", "checkbox", "dropdown", "textarea", "radio"
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? Options { get; set; } // JSON string for dropdown/radio options, e.g., ["Option1", "Option2"]

        // Helper to get/set options as List<string>
        public List<string> GetOptionsList() => string.IsNullOrEmpty(Options) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(Options);
        public void SetOptionsList(List<string> options) => Options = options != null ? JsonSerializer.Serialize(options) : null;
    }
}
