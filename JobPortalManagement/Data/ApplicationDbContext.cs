using JobPortalManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPortalManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<FieldMaster> FieldMasters => Set<FieldMaster>();
        public DbSet<UserRegistration> UserRegistrations => Set<UserRegistration>();
        public DbSet<TblMenu> TblMenus { get; set; }
        public DbSet<TblMenuPermission> TblMenuPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RefreshToken>()
             .HasOne(rt => rt.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
             .HasIndex(rt => rt.Token)
             .IsUnique();
            builder.Entity<TblMenu>().HasKey(m => m.MenuId);
            builder.Entity<TblMenuPermission>().HasKey(mp => mp.PermissionId);

            builder.Entity<TblMenu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId);

            builder.Entity<TblMenuPermission>()
                .HasOne(mp => mp.Menu)
                .WithMany(m => m.Permissions)
                .HasForeignKey(mp => mp.MenuId);

            builder.Entity<TblMenuPermission>()
                .HasOne(mp => mp.Role)
                .WithMany()
                .HasForeignKey(mp => mp.RoleId);
        }
    }
}
