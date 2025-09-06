using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortalManagement.Models
{
    [Table("FieldMaster")]
    public class FieldMaster
    {
        [Key]
        public int FieldId { get; set; }
        public string FieldName { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string CssClass { get; set; } = default!;
        public string FieldType { get; set; } = default!; // text, number, email, date, dropdown, textarea, checkbox
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public int OrderNo { get; set; }
        public string? OptionsJson { get; set; }
    }
    [Table("UserRegistration")]
    public class UserRegistration
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string? DynamicData { get; set; } // use string in EF; db column is JSONB in Postgres
        public DateTime CreatedAt { get; set; }
    }
}
