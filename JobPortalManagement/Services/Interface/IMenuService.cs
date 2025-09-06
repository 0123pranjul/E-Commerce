using JobPortalManagement.Models.DTO;

namespace JobPortalManagement.Services.Interface
{
    public interface IMenuService
    {
        Task<List<MenuItemViewModel>> GetMenusByRoleAsync(string roleId);
    }
}
