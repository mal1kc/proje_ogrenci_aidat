using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SchoolDBSeeder : DbSeeder<AppDbContext, School>
    {
        public SchoolDBSeeder(AppDbContext context, IConfiguration configuration)
            : base(context, configuration) { }

        protected override async Task SeedDataAsync()
        {
            if (_context.Schools == null)
            {
                throw new NullReferenceException("SchoolDBSeeder: _context.Schools is null");
            }
            foreach (var school in _seedData)
            {
                if (await _context.Schools.AnyAsync(s => s.Name == school.Name))
                {
                    continue;
                }

                school.CreatedAt = DateTime.Now;
                school.UpdatedAt = DateTime.Now;

                await _context.Schools.AddAsync(school);
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            if (_context.Schools == null)
            {
                throw new NullReferenceException("SchoolDBSeeder: _context.Schools is null");
            }
            for (int i = 0; i < 5; i++) // Seed 5 random schools
            {
                var school = CreateRandomModel();
                await _context.Schools.AddAsync(school);
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging)
            {
                Console.WriteLine("SchoolDBSeeder: AfterSeedDataAsync");
                Console.WriteLine("We have seed data:");
                foreach (var school in _seedData)
                {
                    Console.WriteLine($"SchoolDBSeeder: AfterSeedDataAsync {school.Name}");
                }
            }

            foreach (var school in _seedData)
            {
                if (!await _context.Schools.AnyAsync(s => s.Name == school.Name))
                {
                    throw new Exception(
                        $"SchoolDBSeeder: AfterSeedDataAsync {school.Name} not found"
                    );
                }
            }
        }

        protected override School CreateRandomModel()
        {
            return new School
            {
                Name = "RandomSchool" + random.Next(100),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = new HashSet<Student>()
            };
        }

        private readonly List<School> _seedData = new List<School>
        {
            new School
            {
                Name = "School 1",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = new HashSet<Student>()
            },
            new School
            {
                Name = "School 2",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = new HashSet<Student>()
            },
            new School
            {
                Name = "School 3",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = new HashSet<Student>()
            }
        };
    }
}
