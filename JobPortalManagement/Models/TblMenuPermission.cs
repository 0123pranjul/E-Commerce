using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortalManagement.Models
{
    [Table("TblMenuPermission")]
    public class TblMenuPermission
    {
        [Key]
        public int PermissionId { get; set; }
        public string RoleId { get; set; }   // AspNetRoles se link hoga
        public int MenuId { get; set; }

        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }

        // Navigation
        public TblMenu Menu { get; set; }
        public IdentityRole Role { get; set; }
    }
}
