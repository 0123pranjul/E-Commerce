using JobPortalManagement.Data;
using JobPortalManagement.Models;
using JobPortalManagement.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JobPortalManagement.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public RoleController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this._context = context;
        }

        public IActionResult Index()
        {
            var vm = new RoleManagementVM
            {
                Roles = _roleManager.Roles.ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleManagementVM model)
        {
          
            if (string.IsNullOrEmpty(model.Role.Id))
            {
                // Insert
                await _roleManager.CreateAsync(new IdentityRole(model.Role.RoleName));
            }
            else
            {
                // Update
                var role = await _roleManager.FindByIdAsync(model.Role.Id);
                if (role != null)
                {
                    role.Name = model.Role.RoleName;
                    await _roleManager.UpdateAsync(role);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            return Json(new { id = role.Id, name = role.Name });
        }
        [HttpGet]
        public async Task<IActionResult> AssignMenu()
        {
            var roles = await _roleManager.Roles
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Id })
                .ToListAsync();

            var menuHierarchy = await GetMenuHierarchy();

            var vm = new AssignMenuPermissionViewModel
            {
                Roles = roles,
                MenuPermissions = menuHierarchy
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> MenuAsign(AssignMenuPermissionViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedRole))
            {
                TempData["Error"] = "Please select a role!";
                return RedirectToAction(nameof(AssignMenu));
            }

            var existing = _context.TblMenuPermissions.Where(mp => mp.RoleId == model.SelectedRole);
            _context.TblMenuPermissions.RemoveRange(existing);

            var flattened = FlattenMenuPermissions(model.MenuPermissions, model.SelectedRole);
            _context.TblMenuPermissions.AddRange(flattened);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Permissions saved successfully!";
            return RedirectToAction(nameof(AssignMenu));
        }

        // Flatten tree structure
        private List<TblMenuPermission> FlattenMenuPermissions(List<MenuPermissionDto> menuPermissions, string roleId)
        {
            var list = new List<TblMenuPermission>();

            void Traverse(List<MenuPermissionDto> items)
            {
                foreach (var item in items)
                {
                    list.Add(new TblMenuPermission
                    {
                        RoleId = roleId,
                        MenuId = item.MenuId,
                        CanRead = item.CanRead,
                        CanCreate = item.CanCreate,
                        CanUpdate = item.CanUpdate,
                        CanDelete = item.CanDelete
                    });

                    if (item.Children != null && item.Children.Any())
                        Traverse(item.Children);
                }
            }

            Traverse(menuPermissions);
            return list;
        }

        // Build menu hierarchy from TblMenu
        private async Task<List<MenuPermissionDto>> GetMenuHierarchy()
        {
            var menus = await _context.TblMenus.Where(m => m.IsActive).ToListAsync();

            List<MenuPermissionDto> BuildTree(int? parentId)
            {
                return menus.Where(m => m.ParentId == parentId)
                    .Select(m => new MenuPermissionDto
                    {
                        MenuId = m.MenuId,
                        MenuName = m.MenuName,
                        Children = BuildTree(m.MenuId)
                    }).ToList();
            }

            return BuildTree(null);
        }
        [HttpGet]
        public async Task<IActionResult> GetMenuPermissionsByRole(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                return BadRequest("RoleId is required");

            // Fetch all menus
            var menus = await _context.TblMenus
                .Include(m => m.Children)
                .Include(m => m.Permissions)
                .Where(m => m.IsActive)
                .ToListAsync();

            // Flatten menu hierarchy for simplicity
            List<MenuPermissionDto> result = new List<MenuPermissionDto>();

            void MapMenuPermissions(TblMenu menu)
            {
                var perm = menu.Permissions.FirstOrDefault(p => p.RoleId == roleId);
                result.Add(new MenuPermissionDto
                {
                    MenuId = menu.MenuId,
                    MenuName = menu.MenuName,
                    CanRead = perm?.CanRead ?? false,
                    CanCreate = perm?.CanCreate ?? false,
                    CanUpdate = perm?.CanUpdate ?? false,
                    CanDelete = perm?.CanDelete ?? false,
                    Children = new List<MenuPermissionDto>()
                });

                foreach (var child in menu.Children)
                {
                    MapMenuPermissions(child);
                }
            }

            // Only top-level menus
            var topMenus = menus.Where(m => m.ParentId == null).ToList();
            foreach (var menu in topMenus)
            {
                MapMenuPermissions(menu);
            }

            return Json(result);
        }
        [HttpGet]
        public IActionResult RoleAsignUser()
        {
            var roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            }).ToList();

            var users = _userManager.Users.ToList();

            var model = new AssignUserRoleViewModel
            {
                Roles = roles,
                Users = users
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAsignUser(AssignUserRoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedRoleId) || model.SelectedUserIds.Count == 0)
            {
                TempData["Error"] = "Please select a role and at least one user.";
                return RedirectToAction(nameof(RoleAsignUser));
            }

            var role = await _roleManager.FindByIdAsync(model.SelectedRoleId);
            if (role == null)
            {
                TempData["Error"] = "Invalid role selected.";
                return RedirectToAction(nameof(RoleAsignUser));
            }

            foreach (var userId in model.SelectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Remove existing roles (single role per user)
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    // Assign new role
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            TempData["Success"] = "Role assigned successfully!";
            return RedirectToAction(nameof(RoleAsignUser));
        }
    
}






}
