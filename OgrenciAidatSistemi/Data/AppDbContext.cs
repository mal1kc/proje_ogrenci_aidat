using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Data
{
    public class AppDbContext : DbContext
    {
        // TODO make users table for only refrence in other tables
        public DbSet<User>? Users { get; set; } // must be ıd table of users
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

        public DbSet<WorkYear>? WorkYears { get; set; }
        public DbSet<Grade>? Grades { get; set; }

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
            modelBuilder
                .Entity<Payment>()
                .HasDiscriminator<string>("PaymentType")
                .HasValue<CashPayment>("Cash")
                .HasValue<CreditCardPayment>("CreditCard");

            // Student and SchoolAdmin are User but they differ from SiteAdmin
            // SA has username they don't

            /* modelBuilder */
            /*     .Entity<User>() */
            /*     .HasDiscriminator<string>("UserType") */
            /*     .HasValue<Student>("Student") */
            /*     .HasValue<SchoolAdmin>("SchoolAdmin") */
            /*     .HasValue<SiteAdmin>("SiteAdmin"); */

            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<int>();

            modelBuilder.Entity<User>().HasIndex(u => u.EmailAddress).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();

            modelBuilder.Entity<Student>().HasIndex(s => s.StudentId).IsUnique();
            modelBuilder.Entity<Student>().Property(s => s.Role).HasDefaultValue(UserRole.Student);

            // TODO implement auto generate student id for using school id etc
            /* modelBuilder.Entity<Student>().Property(s => s.StudentId).ValueGeneratedOnAdd */

            modelBuilder.Entity<SchoolAdmin>().Property(sa => sa.Role).HasDefaultValue(UserRole.SchoolAdmin);
            modelBuilder.Entity<SiteAdmin>().Property(sa => sa.Role).HasDefaultValue(UserRole.SiteAdmin);



            // all the time include school in student and school admin
            modelBuilder.Entity<Student>().HasOne(s => s.School);
            modelBuilder.Entity<SchoolAdmin>().HasOne(s => s.School);

            modelBuilder.Entity<Payment>().HasOne(p => p.Student);


            base.OnModelCreating(modelBuilder);
        }

        /* public override DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TEntity>() */
        /* { */
        /*     return typeof(TEntity) switch */
        /*     { */
        /*         _ => base.Set<TEntity>() */
        /*     }; */
        /* } */
        /**/

    }
}
