using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;

#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SchoolAdminDBSeeder : DbSeeder<AppDbContext, SchoolAdmin>
    {
        public SchoolAdminDBSeeder(AppDbContext context, IConfiguration configuration)
            : base(context, configuration) { }

        protected override async Task SeedDataAsync()
        {
            if (_context.SchoolAdmins == null)
            {
                throw new NullReferenceException("SchoolAdminDBSeeder: _context.SchoolAdmins is null");
            }
            foreach (var schoolAdmin in _seedData)
            {
                if (await _context.Users.AnyAsync(u => u.EmailAddress == schoolAdmin.EmailAddress))
                {
                    continue;
                }

                schoolAdmin.CreatedAt = DateTime.Now;
                schoolAdmin.UpdatedAt = DateTime.Now;

                if (await _context.Schools.AnyAsync(s => s.Name == schoolAdmin.School.Name))
                {
                    schoolAdmin.School = await _context.Schools.FirstAsync(s =>
                        s.Name == schoolAdmin.School.Name
                    );
                }
                else
                {
                    schoolAdmin.School.CreatedAt = DateTime.Now;
                    schoolAdmin.School.UpdatedAt = DateTime.Now;
                    await _context.Schools.AddAsync(schoolAdmin.School);
                }

                await _context.SchoolAdmins.AddAsync(schoolAdmin);
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            if (_context.SchoolAdmins == null)
            {
                throw new NullReferenceException("SchoolAdminDBSeeder: _context.SchoolAdmins is null");
            }
            for (int i = 0; i < 10; i++) // Seed 10 random school admins
            {
                var schoolAdmin = CreateRandomModel();
                Console.WriteLine(
                    $"Generated SchoolAdmin: EmailAddress: {schoolAdmin.EmailAddress}, Password: {"RandomPassword_" + schoolAdmin.EmailAddress.Split('@')[0]}"
                );
                if (await _context.Users.AnyAsync(a => a.EmailAddress == schoolAdmin.EmailAddress))
                {
                    continue;
                }
                await _context.SchoolAdmins.AddAsync(schoolAdmin);
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
                ContactInfo = new ContactInfo
                {
                    Name = "Random Name",
                    Email = email,
                }
            };
        }

        private readonly List<SchoolAdmin> _seedData = new List<SchoolAdmin>
        {

        new SchoolAdmin
            {
                FirstName = "SchoolAdmin1",
                LastName = "SchoolAdmin1",
                EmailAddress = "sch_admin1@school1",
                PasswordHash = SchoolAdmin.ComputeHash("Password1"),
                School = new School
                {
                    Name = "School1",
                    Students = new HashSet<Student>()
                },
                ContactInfo = new ContactInfo
                {
                    Name = "SchoolAdmin2",
                    Email = "sch_admin2@school2",
                    PhoneNumber = "+90 555 555 55 56",
                }
            },

        };
    }
}
