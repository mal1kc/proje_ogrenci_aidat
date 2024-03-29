using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Data
{
    public class SchoolAdminDBSeeder : DbSeeder<AppDbContext, SchoolAdmin>
    {
        public SchoolAdminDBSeeder(AppDbContext context, IConfiguration configuration)
            : base(context, configuration)
        {
            _seedData = new()
            {
                new()
                {
                    Username = "sch_admin",
                    FirstName = "mustafa",
                    LastName = "admin",
                    EmailAddress = "sch_admin123@example.com",
                    PasswordHash = SchoolAdmin.ComputeHash("SchAdmin123"),
                    _School = new() { Name = "School 1" }
                }
            };
            if (_verboselogging)
            {
                Console.WriteLine("created SchoolAdminDBSeeder");
            }
        }

        protected override async Task SeedDataAsync()
        {
            if (_verboselogging)
            {
                Console.WriteLine("SchoolAdminDBSeeder: Seeding SchoolAdmins");
            }

            foreach (var schooladmin in _seedData)
            {
                if (_verboselogging)
                {
                    Console.WriteLine(
                        $"SchoolAdminDBSeeder: Seeding SchoolAdmins {schooladmin.Username}"
                    );
                }

                if (await _context.SchoolAdmins.AnyAsync(a => a.Username == schooladmin.Username))
                {
                    continue;
                }
                schooladmin.CreatedAt = DateTime.Now;
                schooladmin.UpdatedAt = DateTime.Now;
                object value = await _context.SchoolAdmins.AddAsync(schooladmin);
                if (_verboselogging)
                {
                    Console.WriteLine(
                        $"SchoolAdminDBSeeder: Seeding SchoolAdmins {schooladmin.Username} {value}"
                    );
                }
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            throw new NotImplementedException();
        }

        // <summary>
        // generally used for testing is seed data is created correctly
        // </summary>
        protected override async Task AfterSeedDataAsync()
        {
            // search seed data in dbcontext if not raise exception
            if (_verboselogging)
            {
                Console.WriteLine("SchoolAdminDBSeeder: AfterSeedDataAsync");
                Console.WriteLine(" we have seed data");
                for (int i = 0; i < _seedData.Count; i++)
                {
                    Console.WriteLine(
                        $"SchoolAdminDBSeeder: AfterSeedDataAsync {_seedData[i].Username}"
                    );
                }
            }

            foreach (var schooladmin in _seedData)
            {
                if (!await _context.SchoolAdmins.AnyAsync(a => a.Username == schooladmin.Username))
                {
                    throw new Exception(
                        $"SchoolAdminDBSeeder: AfterSeedDataAsync {schooladmin.Username} not found"
                    );
                }
            }
        }
    }
}
