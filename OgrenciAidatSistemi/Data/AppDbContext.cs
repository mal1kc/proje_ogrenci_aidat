using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<SiteAdmin>? SiteAdmins { get; set; }

        // public DbSet<SchoolAdmin>? SchoolAdmins { get; set; }

        // TODO: add other models here

        // TODO: needs to be change in devlopment and production use

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite("DataSource = app_.db; Cache=Shared");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // example
            //     modelBuilder.Entity<Product>()
            //     .HasMany(e => e.Tags)
            //     .WithMany(e => e.products)
            //     .UsingEntity("ProductTag",
            //         l => l.HasOne(typeof(Tag))
            //             .WithMany()
            //             .HasForeignKey("TagId")
            //             .HasPrincipalKey(nameof(Tag.Id)),
            //         r => r.HasOne(typeof(Product))
            //             .WithMany()
            //             .HasForeignKey("ProductId")
            //             .HasPrincipalKey(nameof(Product.Id)),
            //         jo =>
            //         {
            //             jo.HasKey("ProductId", "TagId");
            //         }
            //     );

            // example
            // modelBuilder.Entity<Product>()
            // .HasOne(p => p.Category)
            // .WithMany(pc => pc.products)
            // .HasForeignKey(p => p.CategoryId)
            // .HasPrincipalKey(pc => pc.Id);
        }
    }
}
