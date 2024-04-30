using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SiteAdminDBSeeder : DbSeeder<AppDbContext, SiteAdmin>
    {
        public SiteAdminDBSeeder(AppDbContext context, IConfiguration configuration, ILogger logger)
            : base(context, configuration, logger) { }

        protected override async Task SeedDataAsync()
        {
            if (_context.SiteAdmins == null)
            {
                throw new Exception("SiteAdminDBSeeder: SeedDataAsync _context.SiteAdmins is null");
            }
            var dbCount = await _context.SiteAdmins.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;

            foreach (var siteAdmin in _seedData)
            {
                await SeedEntityAsync(siteAdmin);

                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            if (_context.SiteAdmins == null)
                throw new Exception(
                    "SiteAdminDBSeeder: SeedRandomDataAsync _context.SiteAdmins is null"
                );

            var dbCount = await _context.SiteAdmins.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;

            var siteAdmins = GetSeedData(true);

            foreach (var siteAdmin in siteAdmins)
            {
                Console.WriteLine(
                    $"Generated SiteAdmin: EmailAddress: {siteAdmin.EmailAddress}, Password: RandomPassword_{siteAdmin.EmailAddress}"
                );
                await SeedEntityAsync(siteAdmin);

                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
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

        public override IEnumerable<SiteAdmin> GetSeedData(bool randomSeed = false)
        {
            if (randomSeed)
            {
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            }
            return _seedData;
        }

        protected override async Task SeedEntityAsync(SiteAdmin entity)
        {
            if (await _context.SiteAdmins.AnyAsync(a => a.EmailAddress == entity.EmailAddress))
            {
                return;
            }

            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;

            await _context.SiteAdmins.AddAsync(entity);
            _seedCount++;
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
