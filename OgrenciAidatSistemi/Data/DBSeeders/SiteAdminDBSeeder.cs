using Bogus;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SiteAdminDBSeeder : DbSeeder<AppDbContext, SiteAdmin>
    {
        public SiteAdminDBSeeder(AppDbContext context, IConfiguration configuration, ILogger logger)
            : base(context, configuration, logger)
        {
            _verboseLogging = configuration
                .GetSection("SeedData")
                .GetValue("VerboseLogging", false);

            //  objects with is like this
            // {
            //      "SeedAdmins": true| false
            //      "SeedDB": true| false // doesn't matter if SeedAdmins is true
            //     "SiteAdmins":
            //     [
            //        {
            //          # if DEBUG
            //             "Password" : "axad",
            //           # else
            //              "PasswordHash" : "axad",
            //          # endif
            //              "Username" : "someusername" ,
            //              "email" : "somemeail" } , { }
            //      ]
            // }

            _is_seeding =
                configuration.GetSection("SeedData").GetValue("SeedAdmins", false)
                || configuration.GetSection("SeedData").GetValue("SeedDB", false);

            var seedData = configuration.GetSection("SeedData").GetSection("SiteAdmins");

            _is_seeding = seedData.Exists() || _is_seeding; // seedData exists or SeedAdmins is true

            if (seedData.Exists())
            {
                _seedData.Clear(); // clear any previous data
                foreach (var siteAdmin in seedData.GetChildren())
                {
                    var username =
                        siteAdmin["Username"]
                        ?? throw new Exception("SiteAdminDBSeeder: Username is required");
                    var email =
                        siteAdmin["Email"]
                        ?? throw new Exception("SiteAdminDBSeeder: Email is required");
                    var firstName = siteAdmin["FirstName"] ?? "";
                    _seedData.Add(
                        new SiteAdmin
                        {
                            Username = username,
                            FirstName = firstName,
                            LastName = siteAdmin["LastName"] ?? "",
                            EmailAddress = email,
#if DEBUG
                            PasswordHash = SiteAdmin.ComputeHash(
                                siteAdmin["Password"]
                                    ?? throw new Exception(
                                        "SiteAdminDBSeeder: Password is required"
                                    )
                            )
#else
                            PasswordHash =
                                siteAdmin["PasswordHash"]
                                ?? throw new Exception(
                                    "SiteAdminDBSeeder: PasswordHash is required"
                                )
#endif
                        }
                    );
                }
            }
        }

        private readonly Faker faker = new();

        protected override async Task SeedDataAsync()
        {
            _context.SiteAdmins ??= _context.Set<SiteAdmin>();

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
            _context.SiteAdmins ??= _context.Set<SiteAdmin>();

            var dbCount = await _context.SiteAdmins.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;

            var siteAdmins = GetSeedData();

            foreach (var siteAdmin in siteAdmins)
            {
                _logger.LogInformation(
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
                _logger.LogInformation("SiteAdminDBSeeder: AfterSeedDataAsync");
                _logger.LogInformation("We have seed data:");
                foreach (var siteAdmin in _seedData)
                {
                    _logger.LogInformation(
                        $"SiteAdminDBSeeder: AfterSeedDataAsync {siteAdmin.EmailAddress}"
                    );
                }
            }
            if (_randomSeed)
                return;

            foreach (var siteAdmin in _seedData)
            {
                if (
                    !await _context.SiteAdmins.AnyAsync(a =>
                        a.EmailAddress == siteAdmin.EmailAddress
                    )
                )
                    throw new Exception(
                        $"SiteAdminDBSeeder: AfterSeedDataAsync {siteAdmin.EmailAddress} not found"
                    );
            }
        }

        protected override SiteAdmin CreateRandomModel()
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();

            var email_nm = $"{firstName.ToLower()}_{lastName.ToLower()}";
            var password = "RandomPassword_" + email_nm; // gitleaks:allow
            return new SiteAdmin
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = $"{email_nm}@random_generated.com",
                PasswordHash = SiteAdmin.ComputeHash(password)
            };
        }

        public override IEnumerable<SiteAdmin> GetSeedData()
        {
            if (_randomSeed)
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            return _seedData;
        }

        public override async Task SeedEntityAsync(SiteAdmin entity)
        {
            if (await _context.SiteAdmins.AnyAsync(a => a.EmailAddress == entity.EmailAddress))
                return;

            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SiteAdmins.AddAsync(entity);
            _seedCount++;
        }

        private List<SiteAdmin> _seedData =
        [
            new SiteAdmin
            {
                Username = "mal1kc",
                FirstName = "malik",
                LastName = "",
                EmailAddress = "admin@example.com",
                PasswordHash = SiteAdmin.ComputeHash("Aadmin123")
            }
        ];
    }
}
