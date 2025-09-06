using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace JobPortalManagement.Models.DTO
{
    public class RoleManagementVM
    {
        public List<IdentityRole> Roles { get; set; } = new();
        public RoleViewModel? Role { get; set; } = new RoleViewModel();
    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
    }
    public class MenuPermissionDto
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public List<MenuPermissionDto> Children { get; set; } = new List<MenuPermissionDto>();
    }

    public class AssignMenuPermissionViewModel
    {
        public string SelectedRole { get; set; }
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        public List<MenuPermissionDto> MenuPermissions { get; set; } = new List<MenuPermissionDto>();
    }
    public class BulkUserRoleAssignViewModel
    {
        public string SelectedRoleId { get; set; }
        public List<SelectListItem> Roles { get; set; } = new();

        public List<UserCheckBoxDto> Users { get; set; } = new();
    }

    public class UserCheckBoxDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
    public class AssignUserRoleViewModel
    {
        public List<SelectListItem> Roles { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<string> SelectedUserIds { get; set; } = new();
        public string SelectedRoleId { get; set; }
    }
    public class MenuItemViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public List<MenuItemViewModel> Children { get; set; } = new List<MenuItemViewModel>();
    }

}
