using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SchoolAdminDBSeeder : DbSeeder<AppDbContext, SchoolAdmin>
    {
        public SchoolAdminDBSeeder(
            AppDbContext context,
            IConfiguration configuration,
            ILogger logger
        )
            : base(context, configuration, logger) { }

        protected override async Task SeedDataAsync()
        {
            if (_context.SchoolAdmins == null)
                throw new NullReferenceException(
                    "SchoolAdminDBSeeder: _context.SchoolAdmins is null"
                );
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
            if (_context.SchoolAdmins == null)
            {
                throw new NullReferenceException(
                    "SchoolAdminDBSeeder: _context.SchoolAdmins is null"
                );
            }
            var dbCount = await _context.SchoolAdmins.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;
            var schoolAdmins = GetSeedData(true);
            foreach (var schoolAdmin in schoolAdmins)
            {
                Console.WriteLine(
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
                Console.WriteLine("SchoolAdminDBSeeder: AfterSeedDataAsync");
                Console.WriteLine("We have seed data:");
                foreach (var schoolAdmin in _seedData)
                {
                    Console.WriteLine(
                        $"SchoolAdminDBSeeder: AfterSeedDataAsync {schoolAdmin.EmailAddress}"
                    );
                }
            }

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
            var email = $"rnd_ml_{random.Next(100)}@example.com";
            return new SchoolAdmin
            {
                FirstName = "rschAdmin" + RandomizerHelper.GenerateRandomString(random.Next(2, 10)),
                LastName = "rschAdmin" + RandomizerHelper.GenerateRandomString(random.Next(2, 10)),
                EmailAddress = email,
                PasswordHash = SchoolAdmin.ComputeHash("RandomPassword_" + email.Split('@')[0]),
                School = new School
                {
                    Name = "School" + RandomizerHelper.GenerateRandomString(random.Next(2, 10)),
                    Students = new HashSet<Student>()
                },
                ContactInfo = new ContactInfo { Email = email, }
            };
        }

        public override IEnumerable<SchoolAdmin> GetSeedData(bool randomSeed = false)
        {
            if (randomSeed)
            {
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            }
            return _seedData;
        }

        protected override async Task SeedEntityAsync(SchoolAdmin entity)
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
                School = new School { Name = "School1", Students = new HashSet<Student>() },
                ContactInfo = new ContactInfo
                {
                    Email = "sch_admin2@school2",
                    PhoneNumber = "+90 555 555 55 56",
                }
            },
        };
    }
}
