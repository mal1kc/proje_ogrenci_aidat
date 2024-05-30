using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // DbSet properties for various entities
        public DbSet<User> Users { get; set; }
        public DbSet<SiteAdmin> SiteAdmins { get; set; }
        public DbSet<SchoolAdmin> SchoolAdmins { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<ContactInfo> Contacts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentPeriod> PaymentPeriods { get; set; }
        public DbSet<CashPayment> CashPayments { get; set; }
        public DbSet<BankPayment> BankPayments { get; set; }
        public DbSet<CreditCardPayment> DebitCardPayments { get; set; }
        public DbSet<CheckPayment> CheckPayments { get; set; }
        public DbSet<UnPaidPayment> UnPaidPayments { get; set; }
        public DbSet<WorkYear> WorkYears { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use SQLite as default provider if no other is configured
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(
                    "Data Source=app.db",
                    options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // disable nullable reference types warnings
#pragma warning disable CS8604 , CS8634, CS8602 , CS8621 , CS8622
            // Configure inheritance for payments
            modelBuilder.Entity<CashPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<BankPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<CheckPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<CreditCardPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<UnPaidPayment>().HasBaseType<Payment>();

            // Configure role properties for User and derived types
            modelBuilder
                .Entity<User>()
                .Property(u => u.Role)
                .HasConversion<int>()
                .ValueGeneratedNever();
            modelBuilder
                .Entity<Student>()
                .Property(s => s.Role)
                .HasDefaultValue(UserRole.Student)
                .ValueGeneratedNever();
            modelBuilder
                .Entity<SchoolAdmin>()
                .Property(sa => sa.Role)
                .HasDefaultValue(UserRole.SchoolAdmin)
                .ValueGeneratedNever();
            modelBuilder
                .Entity<SiteAdmin>()
                .Property(sa => sa.Role)
                .HasDefaultValue(UserRole.SiteAdmin)
                .ValueGeneratedNever();

            // Configure discriminator column for Payment types
            modelBuilder
                .Entity<Payment>()
                .HasDiscriminator<PaymentMethod>("PaymentMethod")
                .HasValue<CashPayment>(PaymentMethod.Cash)
                .HasValue<BankPayment>(PaymentMethod.Bank)
                .HasValue<CreditCardPayment>(PaymentMethod.CreditCard)
                .HasValue<CheckPayment>(PaymentMethod.Check)
                .HasValue<UnPaidPayment>(PaymentMethod.UnPaid);

            // Unique indexes for Users and Students
            modelBuilder.Entity<User>().HasIndex(u => u.EmailAddress).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<Student>().HasIndex(s => s.StudentId).IsUnique();

            // Configure relationships
            modelBuilder.Entity<Student>().HasOne(s => s.School);
            modelBuilder.Entity<SchoolAdmin>().HasOne(sa => sa.School);
            modelBuilder.Entity<Payment>().HasOne(p => p.Student);
            modelBuilder.Entity<Payment>().HasOne(p => p.PaymentPeriod).WithMany(pp => pp.Payments);
            modelBuilder.Entity<Payment>().HasOne(p => p.Receipt);
            modelBuilder.Entity<Payment>().HasOne(p => p.Student).WithMany(s => s.Payments)
            // .OnDelete(DeleteBehavior.SetNull);
            ;
            modelBuilder
                .Entity<WorkYear>()
                .HasMany(wy => wy.PaymentPeriods)
                .WithOne(pp => pp.WorkYear);
            modelBuilder.Entity<WorkYear>().HasOne(wy => wy.School).WithMany(s => s.WorkYears);
            modelBuilder.Entity<PaymentPeriod>().HasOne(pp => pp.WorkYear);

            modelBuilder.Entity<School>().HasMany(s => s.Students).WithOne(s => s.School)
            // .OnDelete(DeleteBehavior.Cascade)
            ;
            modelBuilder.Entity<School>().HasMany(s => s.SchoolAdmins).WithOne(sa => sa.School)
            // .OnDelete(DeleteBehavior.Cascade)
            ;
            modelBuilder.Entity<School>().HasMany(s => s.WorkYears).WithOne(wy => wy.School)
            // .OnDelete(DeleteBehavior.Cascade)
            ;
            modelBuilder.Entity<School>().HasMany(s => s.Grades).WithOne(g => g.School)
            // .OnDelete(DeleteBehavior.Cascade)
            ;
            // Delete behaviors for other relationships
            modelBuilder
                .Entity<WorkYear>()
                .HasMany(wy => wy.PaymentPeriods)
                .WithOne(pp => pp.WorkYear)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            modelBuilder.Entity<Student>().HasMany(s => s.Payments).WithOne(p => p.Student)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            modelBuilder.Entity<Student>().HasMany(s => s.PaymentPeriods).WithOne(pp => pp.Student)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            modelBuilder.Entity<Payment>().HasOne(p => p.PaymentPeriod).WithMany(pp => pp.Payments)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            modelBuilder
                .Entity<PaymentPeriod>()
                .HasOne(pp => pp.WorkYear)
                .WithMany(wy => wy.PaymentPeriods)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            modelBuilder.Entity<Payment>().HasOne(p => p.Receipt).WithOne(r => r.Payment)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            modelBuilder.Entity<Receipt>().HasOne(r => r.Payment).WithOne(p => p.Receipt)
            // .OnDelete(DeleteBehavior.SetNull)
            ;
            // for mssql
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            modelBuilder
                .Entity<PaymentPeriod>()
                .Property(pp => pp.TotalAmount)
                .HasColumnType("decimal(18,2)");
            modelBuilder
                .Entity<PaymentPeriod>()
                .Property(pp => pp.PerPaymentAmount)
                .HasColumnType("decimal(18,2)");

            // Default values and update behaviors for timestamps

            var entityTypes = new List<Type>()
            {
                typeof(User),
                typeof(SchoolAdmin),
                typeof(Student),
                typeof(School),
                typeof(ContactInfo),
                typeof(Payment),
                typeof(PaymentPeriod),
                typeof(CashPayment),
                typeof(BankPayment),
                typeof(CreditCardPayment),
                typeof(CheckPayment),
                typeof(UnPaidPayment),
                typeof(WorkYear),
                typeof(Grade),
                typeof(Receipt)
            };

            foreach (var entityType in entityTypes)
            {
                modelBuilder
                    .Entity(entityType)
                    .Property<DateTime>("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder
                    .Entity(entityType)
                    .Property<DateTime>("UpdatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }

#pragma warning restore CS8604 , CS8634, CS8602 , CS8621 , CS8622

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                // Calculate total amount for PaymentPeriod
                if (entry.Entity is PaymentPeriod pp)
                {
                    pp.TotalAmount = pp.Payments?.Sum(p => p.Amount) ?? 0;
                }
            }

            return base.SaveChanges();
        }
    }
}
