using Bogus;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SchoolAdminDBSeeder(
        AppDbContext context,
        IConfiguration configuration,
        ILogger logger,
        int maxSeedCount = 100,
        bool randomSeed = false
    )
        : DbSeeder<AppDbContext, SchoolAdmin>(
            context,
            configuration,
            logger,
            maxSeedCount,
            randomSeed
        )
    {
        private readonly Faker faker = new("tr");

        protected override async Task SeedDataAsync()
        {
            _context.SchoolAdmins ??= _context.Set<SchoolAdmin>();
            var dbCount = await _context.SchoolAdmins.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;
            foreach (var schoolAdmin in _seedData)
            {
                await SeedEntityAsync(schoolAdmin);
                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            _context.SchoolAdmins ??= _context.Set<SchoolAdmin>();

            var dbCount = await _context.SchoolAdmins.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;
            var schoolAdmins = GetSeedData();
            foreach (var schoolAdmin in schoolAdmins)
            {
                _logger.LogInformation(
                    $"Generated SchoolAdmin: EmailAddress: {schoolAdmin.EmailAddress}, Password: {"RandomPassword_" + schoolAdmin.EmailAddress.Split('@')[0]}"
                );
                await SeedEntityAsync(schoolAdmin);
                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
            }
            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging)
            {
                _logger.LogInformation("SchoolAdminDBSeeder: AfterSeedDataAsync");
                _logger.LogInformation("We have seed data:");
                foreach (var schoolAdmin in _seedData)
                {
                    _logger.LogInformation(
                        $"SchoolAdminDBSeeder: AfterSeedDataAsync {schoolAdmin.EmailAddress}"
                    );
                }
            }

            if (_randomSeed)
                return;

            foreach (var schoolAdmin in _seedData)
            {
                if (
                    !await _context.SchoolAdmins.AnyAsync(a =>
                        a.EmailAddress == schoolAdmin.EmailAddress
                    )
                )
                {
                    throw new Exception(
                        $"SchoolAdminDBSeeder: AfterSeedDataAsync {schoolAdmin.EmailAddress} not found"
                    );
                }
            }
        }

        protected override SchoolAdmin CreateRandomModel()
        {
            var email = $"rnd_ml_{faker.Random.Number(1, 100)}@example.com";
            return new SchoolAdmin
            {
                FirstName = "rschAdmin" + faker.Name.FirstName(),
                LastName = "rschAdmin" + faker.Name.LastName(),
                EmailAddress = email,
                PasswordHash = SchoolAdmin.ComputeHash("RandomPassword_" + email.Split('@')[0]),
                School = new School
                {
                    Name = "School" + faker.Random.Number(1, 100),
                    Students = null
                },
                ContactInfo = new ContactInfo { Email = email, }
            };
        }

        public override IEnumerable<SchoolAdmin> GetSeedData()
        {
            if (_randomSeed)
            {
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            }
            return _seedData;
        }

        public override async Task SeedEntityAsync(SchoolAdmin entity)
        {
            if (await _context.SchoolAdmins.AnyAsync(a => a.EmailAddress == entity.EmailAddress))
                return;

            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;
            if (entity.School != null)
            {
                var dbSchool = await _context.Schools.FirstOrDefaultAsync(s =>
                    s.Name == entity.School.Name
                );
                entity.School.CreatedAt = DateTime.Now;
                if (dbSchool != null)
                {
                    entity.School = dbSchool;
                }
                entity.School.UpdatedAt = DateTime.Now;
            }
            await _context.SchoolAdmins.AddAsync(entity);
            _seedCount++;
        }

        private readonly List<SchoolAdmin> _seedData = new List<SchoolAdmin>
        {
            new SchoolAdmin
            {
                FirstName = "SchoolAdmin1",
                LastName = "SchoolAdmin1",
                EmailAddress = "sch_admin1@school1",
                PasswordHash = SchoolAdmin.ComputeHash("Password1"),
                School = new School { Name = "School1", Students = null },
                ContactInfo = new ContactInfo
                {
                    Email = "sch_admin2@school2",
                    PhoneNumber = "+90 555 555 55 56",
                }
            },
        };
    }
}
