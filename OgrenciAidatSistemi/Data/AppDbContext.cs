using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } // must be id table of users
        public DbSet<SiteAdmin> SiteAdmins { get; set; }

        public DbSet<SchoolAdmin> SchoolAdmins { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<School> Schools { get; set; }

        public DbSet<ContactInfo> Contacts { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentPeriod> PaymentPeriods { get; set; }

        public DbSet<CashPayment> CashPayments { get; set; }

        public DbSet<BankPayment> BankPayments { get; set; }
        public DbSet<DebitCardPayment> DebitCardPayments { get; set; }

        public DbSet<CheckPayment> CheckPayments { get; set; }

        public DbSet<UnPaidPayment> NonPaidPayments { get; set; }

        public DbSet<WorkYear> WorkYears { get; set; }
        public DbSet<Grade> Grades { get; set; }

        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // if no provider is not set use sqlite
            if (!optionsBuilder.IsConfigured)
            {
                // TODO: needs to be changed in development and production use
                optionsBuilder.UseSqlite("Data Source=app.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CashPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<BankPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<CheckPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<DebitCardPayment>().HasBaseType<PaidPayment>();
            modelBuilder.Entity<UnPaidPayment>().HasBaseType<Payment>();

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

            // Add configurations for other derived types if applicable

            // Configure discriminator column (if needed)
            modelBuilder
                .Entity<Payment>()
                .HasDiscriminator<PaymentMethod>("PaymentMethod")
                .HasValue<CashPayment>(PaymentMethod.Cash)
                .HasValue<BankPayment>(PaymentMethod.Bank)
                .HasValue<DebitCardPayment>(PaymentMethod.DebitCard)
                .HasValue<CheckPayment>(PaymentMethod.Check)
                .HasValue<UnPaidPayment>(PaymentMethod.UnPaid);

            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<int>();

            modelBuilder.Entity<User>().HasIndex(u => u.EmailAddress).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();

            modelBuilder.Entity<Student>().HasIndex(s => s.StudentId).IsUnique();

            // TODO implement auto generate student id for using school id etc
            /* modelBuilder.Entity<Student>().Property(s => s.StudentId).ValueGeneratedOnAdd */


            // all the time include school in student and school admin
            modelBuilder.Entity<Student>().HasOne(s => s.School);
            modelBuilder.Entity<SchoolAdmin>().HasOne(s => s.School);

            modelBuilder.Entity<Payment>().HasOne(p => p.Student);
            modelBuilder.Entity<Payment>().HasOne(p => p.PaymentPeriod).WithMany(pp => pp.Payments);
            modelBuilder.Entity<Payment>().HasOne(p => p.Receipt);

            modelBuilder
                .Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Payments)
                .OnDelete(DeleteBehavior.SetNull); // if student deleted set null to student field of payment

            modelBuilder
                .Entity<WorkYear>()
                .HasMany(pp => pp.PaymentPeriods)
                .WithOne(pp => pp.WorkYear);

            modelBuilder.Entity<WorkYear>().HasOne(wy => wy.School).WithMany(s => s.WorkYears);

            modelBuilder.Entity<PaymentPeriod>().HasOne(pp => pp.WorkYear);

            modelBuilder.Entity<Receipt>().HasIndex(fp => fp.Path).IsUnique();

            // delete all related entities when deleting a school
            modelBuilder
                .Entity<School>()
                .HasMany(s => s.Students)
                .WithOne(s => s.School)
                .OnDelete(DeleteBehavior.Cascade); // delete all students of school

            modelBuilder
                .Entity<School>()
                .HasMany(s => s.SchoolAdmins)
                .WithOne(sa => sa.School)
                .OnDelete(DeleteBehavior.Cascade); // delete all school admins of school

            modelBuilder
                .Entity<School>()
                .HasMany(s => s.WorkYears)
                .WithOne(wy => wy.School)
                .OnDelete(DeleteBehavior.Cascade); // delete all work years of school

            modelBuilder
                .Entity<School>()
                .HasMany(s => s.Grades)
                .WithOne(g => g.School)
                .OnDelete(DeleteBehavior.Cascade); // delete all grades of school

            modelBuilder
                .Entity<WorkYear>()
                .HasMany(wy => wy.PaymentPeriods)
                .WithOne(pp => pp.WorkYear)
                .OnDelete(DeleteBehavior.SetNull); // if work year deleted set null to work year field of payment period

            // set others (not cascaded ones) of school to null if school deleted

            modelBuilder
                .Entity<Student>()
                .HasMany(s => s.Payments)
                .WithOne(p => p.Student)
                .OnDelete(DeleteBehavior.SetNull); // if student deleted set null to student field of payment

            modelBuilder
                .Entity<Student>()
                .HasMany(s => s.PaymentPeriods)
                .WithOne(pp => pp.Student)
                .OnDelete(DeleteBehavior.SetNull); // if student deleted set null to student field of payment period

            // when we delete a payment set null to payment period field of payment
            modelBuilder
                .Entity<Payment>()
                .HasOne(p => p.PaymentPeriod)
                .WithMany(pp => pp.Payments)
                .OnDelete(DeleteBehavior.SetNull); // if payment deleted set null to payment period field of payment

            // when we delete a payment period set null to work year field of payment period
            modelBuilder
                .Entity<PaymentPeriod>()
                .HasOne(pp => pp.WorkYear)
                .WithMany(wy => wy.PaymentPeriods)
                .OnDelete(DeleteBehavior.SetNull); // if payment period deleted set null to work year field of payment period

            // if we delete payment we don't want to delete receipt
            modelBuilder
                .Entity<Payment>()
                .HasOne(p => p.Receipt)
                .WithOne(r => r.Payment)
                .OnDelete(DeleteBehavior.SetNull); // if payment deleted set null to payment field of receipt

            // if we delete a payment set null to receipt field of receipt
            modelBuilder
                .Entity<Receipt>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.Receipt)
                .OnDelete(DeleteBehavior.SetNull); // if receipt deleted set null to payment field of receipt

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.Now;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                // if entity is payment period update total amount

                if (entry.Entity is PaymentPeriod pp)
                {
                    pp.TotalAmount = 0;
                    if (pp.Payments == null)
                        continue;
                    for (int i = 0; i < pp.Payments.Count; i++)
                    {
                        if (pp.Payments.ElementAt(i) != null)
                        {
                            pp.TotalAmount += pp.Payments.ElementAt(i).Amount;
                        }
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}
