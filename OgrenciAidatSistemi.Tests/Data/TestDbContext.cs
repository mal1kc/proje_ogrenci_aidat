using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Tests.Data;

namespace OgrenciAidatSistemi.Tests
{
    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestItem> TestItems { get; set; }

        public DbSet<TestSecondItem> TestSecondItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestItem>().ToTable("TestItems");
            modelBuilder.Entity<TestSecondItem>().ToTable("TestSecondItems");
        }
    }
}
