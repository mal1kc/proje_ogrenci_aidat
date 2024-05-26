using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class SchoolDBSeeder(
        AppDbContext context,
        IConfiguration configuration,
        ILogger logger,
        int maxSeedCount = 20,
        bool randomSeed = false
    ) : DbSeeder<AppDbContext, School>(context, configuration, logger, maxSeedCount, randomSeed)
    {
        private readonly Random random = new();

        protected override async Task SeedDataAsync()
        {
            _context.Schools ??= _context.Set<School>();

            var dbCount = await _context.Schools.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;

            foreach (var school in _seedData)
            {
                await SeedEntityAsync(school);
                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            _context.Schools ??= _context.Set<School>();

            var dbCount = await _context.Schools.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;
            var schools = GetSeedData();
            foreach (var school in schools)
            {
                await SeedEntityAsync(school);
                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging)
            {
                _logger.LogInformation("SchoolDBSeeder: AfterSeedDataAsync");
                _logger.LogInformation("We have seed data:");
                foreach (var school in _seedData)
                {
                    _logger.LogInformation($"SchoolDBSeeder: AfterSeedDataAsync {school.Name}");
                }
            }

            if (_randomSeed)
                return;

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
                Students = null
            };
        }

        public override IEnumerable<School> GetSeedData()
        {
            if (_randomSeed)
            {
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            }
            return _seedData;
        }

        public override async Task SeedEntityAsync(School entity)
        {
            if (await _context.Schools.AnyAsync(s => s.Name == entity.Name))
            {
                return;
            }

            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;

            await _context.Schools.AddAsync(entity);
            _seedCount++;
        }

        private readonly List<School> _seedData =
        [
            new()
            {
                Name = "School 1",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = null
            },
            new()
            {
                Name = "School 2",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = null
            },
            new()
            {
                Name = "School 3",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Students = null
            }
        ];
    }
}
