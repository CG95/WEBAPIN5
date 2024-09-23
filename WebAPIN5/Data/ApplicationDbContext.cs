using Microsoft.EntityFrameworkCore;
using WebAPIN5.Models;

namespace WebAPIN5.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PermissionType> PermissionTypes { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<Permission>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.PermissionType)
                .WithMany(pt => pt.Permissions)
                .HasForeignKey(p => p.PermissionTypeId);

            modelBuilder.Entity<PermissionType>()
                .Property(pt => pt.Id)
                .ValueGeneratedOnAdd();

            // Seed data for PermissionType
            modelBuilder.Entity<PermissionType>().HasData(
                new PermissionType { Id = 1, Description = "Sick Leave" },
                new PermissionType { Id = 2, Description = "Vacation" }
            );

            // Seed data for Permission
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, EmployeeForename = "John", EmployeeSurname = "Doe", PermissionTypeId = 1, PermissionDate = DateTime.Now },
                new Permission { Id = 2, EmployeeForename = "Jane", EmployeeSurname = "Doe", PermissionTypeId = 2, PermissionDate = DateTime.Now }
            );
        }
    }

}