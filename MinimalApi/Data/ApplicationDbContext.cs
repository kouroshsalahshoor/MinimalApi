using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Models;
using MinimalApi.Models.Auth;

namespace MinimalApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    var now = DateTime.Now;

        //    modelBuilder.Entity<Category>().HasData(
        //       new Category() { Id = 1, Name = "1", CreatedBy = "1", CreatedOn = now },
        //       new Category() { Id = 2, Name = "2", CreatedBy = "2", CreatedOn = now },
        //       new Category() { Id = 3, Name = "3", CreatedBy = "3", CreatedOn = now }
        //    );
        //}
    }
}
