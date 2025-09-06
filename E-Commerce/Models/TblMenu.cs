using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortalManagement.Models
{
    [Table("TblMenu")]
    public class TblMenu
    {
        [Key]
        public int MenuId { get; set; }
        public int? ParentId { get; set; }   // Self-reference
        public string MenuName { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        // Navigation
        public TblMenu? Parent { get; set; }
        public ICollection<TblMenu> Children { get; set; } = new List<TblMenu>();
        public ICollection<TblMenuPermission> Permissions { get; set; } = new List<TblMenuPermission>();

    }
}
