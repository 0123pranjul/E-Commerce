using JobPortalManagement.Data;
using JobPortalManagement.Models.DTO;
using JobPortalManagement.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortalManagement.Services
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _db;
        public MenuService(ApplicationDbContext db) { _db = db; }

        public async Task<List<MenuItemViewModel>> GetMenusByRoleAsync(string roleId)
        {
            // Get all menu permissions for this role where CanRead = true
            var menus = await _db.TblMenuPermissions
                .Where(mp => mp.RoleId == roleId && mp.CanRead)
                .Select(mp => mp.Menu)
                .ToListAsync();

            // Recursive function to build hierarchy
            List<MenuItemViewModel> BuildHierarchy(int? parentId)
            {
                return menus
                    .Where(m => m.ParentId == parentId)
                    .Select(m => new MenuItemViewModel
                    {
                        MenuId = m.MenuId,
                        MenuName = m.MenuName,
                        ControllerName = m.ControllerName,
                        ActionName = m.ActionName,
                        Url = m.Url,
                        Icon = m.Icon,
                        Children = BuildHierarchy(m.MenuId) // recursion
                    })
                    .ToList();
            }

            // Start with top-level menus (ParentId == null)
            var parentMenus = BuildHierarchy(null);

            return parentMenus;
        }
    }
}
