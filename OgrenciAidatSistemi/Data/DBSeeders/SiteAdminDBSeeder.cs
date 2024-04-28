using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SiteAdminDBSeeder : DbSeeder<AppDbContext, SiteAdmin>
    {
        public SiteAdminDBSeeder(AppDbContext context, IConfiguration configuration)
            : base(context, configuration) { }

        protected override async Task SeedDataAsync()
        {
            foreach (var siteAdmin in _seedData)
            {
                if (
                    await _context.SiteAdmins.AnyAsync(a =>
                        a.EmailAddress == siteAdmin.EmailAddress
                    )
                )
                {
                    continue;
                }

                siteAdmin.CreatedAt = DateTime.Now;
                siteAdmin.UpdatedAt = DateTime.Now;

                await _context.SiteAdmins.AddAsync(siteAdmin);
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            if (_context.SiteAdmins == null)
            {
                throw new Exception(
                    "SiteAdminDBSeeder: SeedRandomDataAsync _context.SiteAdmins is null"
                );
            }
            for (int i = 0; i < 10; i++) // Seed 10 random site admins
            {
                var siteAdmin = CreateRandomModel();
                Console.WriteLine(
                    $"Generated SiteAdmin: EmailAddress: {siteAdmin.EmailAddress}, Password: RandomPassword_{siteAdmin.EmailAddress}"
                );
                if (await _context.Users.AnyAsync(u => u.EmailAddress == siteAdmin.EmailAddress))
                {
                    continue;
                }
                await _context.SiteAdmins.AddAsync(siteAdmin);
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging)
            {
                Console.WriteLine("SiteAdminDBSeeder: AfterSeedDataAsync");
                Console.WriteLine("We have seed data:");
                foreach (var siteAdmin in _seedData)
                {
                    Console.WriteLine(
                        $"SiteAdminDBSeeder: AfterSeedDataAsync {siteAdmin.EmailAddress}"
                    );
                }
            }

            foreach (var siteAdmin in _seedData)
            {
                if (
                    !await _context.SiteAdmins.AnyAsync(a =>
                        a.EmailAddress == siteAdmin.EmailAddress
                    )
                )
                {
                    throw new Exception(
                        $"SiteAdminDBSeeder: AfterSeedDataAsync {siteAdmin.EmailAddress} not found"
                    );
                }
            }
        }

        protected override SiteAdmin CreateRandomModel()
        {
            var email_nm = "random_user" + random.Next(100);
            var password = "RandomPassword_" + email_nm; // gitleaks:allow
            return new SiteAdmin
            {
                FirstName = "rnd_fn_" + RandomizerHelper.GenerateRandomString(5),
                LastName = "rnd_ln_" + RandomizerHelper.GenerateRandomString(5),
                EmailAddress = $"{email_nm}@example.com",
                PasswordHash = SiteAdmin.ComputeHash(password)
            };
        }

        private readonly List<SiteAdmin> _seedData = new List<SiteAdmin>
        {
            new SiteAdmin
            {
                Username = "mal1kc",
                FirstName = "malik",
                LastName = "",
                EmailAddress = "admin@example.com",
                PasswordHash = SiteAdmin.ComputeHash("Aadmin123")
            }
        };
    }
}
