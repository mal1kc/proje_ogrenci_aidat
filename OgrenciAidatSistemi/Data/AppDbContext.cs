using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User>? Users { get; set; }
        public DbSet<SiteAdmin>? SiteAdmins { get; set; }

        public DbSet<SchoolAdmin>? SchoolAdmins { get; set; }
        public DbSet<Student>? Students { get; set; }
        public DbSet<School>? Schools { get; set; }

        public DbSet<Payment>? Payments { get; set; }
        public DbSet<PaymentPeriode>? PaymentPeriods { get; set; }

        public DbSet<CashPayment>? CashPayments { get; set; }
        public DbSet<CreditCardPayment>? CreditCardPayments { get; set; }
        public DbSet<CheckPayment>? CheckPayments { get; set; }
        public DbSet<DebitCardPayment>? DebitCardPayments { get; set; }
        public DbSet<BankTransferPayment>? BankTransferPayments { get; set; }

        // TODO: needs to be change in devlopment and production use

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite("DataSource = app_.db; Cache=Shared");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CashPayment>().HasBaseType<Payment>();
            modelBuilder.Entity<CreditCardPayment>().HasBaseType<Payment>();
            modelBuilder.Entity<CheckPayment>().HasBaseType<Payment>();
            modelBuilder.Entity<DebitCardPayment>().HasBaseType<Payment>();
            modelBuilder.Entity<BankTransferPayment>().HasBaseType<Payment>();

            // Add configurations for other derived types if applicable

            // Configure discriminator column (if needed)
            modelBuilder.Entity<Payment>().HasDiscriminator<string>("PaymentType")
                                   .HasValue<CashPayment>("Cash")
                                   .HasValue<CreditCardPayment>("CreditCard");




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
