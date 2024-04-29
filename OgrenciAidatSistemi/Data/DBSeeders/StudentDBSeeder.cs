using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class StudentDBSeeder : DbSeeder<AppDbContext, Student>
    {
        public StudentDBSeeder(AppDbContext context, IConfiguration configuration, ILogger logger)
            : base(context, configuration, logger) { }

        protected override async Task SeedDataAsync()
        {
            if (_context.Students == null)
            {
                throw new NullReferenceException("StudentDBSeeder: _context.Students is null");
            }
            foreach (var student in _seedData)
            {
                await SeedEntityAsync(student);
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            var students = GetSeedData(true);
            foreach (var student in students)
            {
                await SeedEntityAsync(student);
            }
            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging)
            {
                Console.WriteLine("StudentDBSeeder: AfterSeedDataAsync");
                Console.WriteLine("We have seed data:");
                foreach (var student in _seedData)
                {
                    Console.WriteLine(
                        $"StudentDBSeeder: AfterSeedDataAsync StudentId: {student.StudentId}"
                    );
                }
            }

            foreach (var student in _seedData)
            {
                if (!await _context.Students.AnyAsync(s => s.StudentId == student.StudentId))
                {
                    throw new Exception(
                        $"StudentDBSeeder: AfterSeedDataAsync StudentId: {student.StudentId} not found"
                    );
                }
            }
        }

        protected override Student CreateRandomModel()
        {
            var studentId = random.Next(1000, 9999);
            return new Student
            {
                StudentId = studentId,
                School = new School
                {
                    Name = "RandomSchool" + random.Next(100),
                    Students = new HashSet<Student>()
                },
                GradLevel = random.Next(1, 13),
                IsGraduated = random.Next(2) == 0, // Generate a random graduation status
                PasswordHash = "password", // dont overthink it
                EmailAddress = $"{studentId}@randomschol.com"
            };
        }

        public override IEnumerable<Student> GetSeedData(bool randomSeed = false)
        {
            if (randomSeed)
            {
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            }
            return _seedData;
        }

        protected override async Task SeedEntityAsync(Student entity)
        {
            if (await _context.Students.AnyAsync(s => s.StudentId == entity.StudentId))
            {
                return;
            }

            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;

            School? assumed_sch = await _context.Schools.FirstAsync(s =>
                s.Name == entity.School.Name
            );

            if (_verboseLogging)
            {
                Console.WriteLine($"StudentDBSeeder: SeedEntityAsync assumed_sch: {assumed_sch}");
            }

            if (assumed_sch != null)
            {
                entity.School = assumed_sch;
            }
            else
            {
                entity.School.CreatedAt = DateTime.Now;
                entity.School.UpdatedAt = DateTime.Now;
                await _context.Schools.AddAsync(entity.School);
            }

            await _context.Students.AddAsync(entity);
        }

        private readonly List<Student> _seedData = new List<Student>
        {
            new Student
            {
                FirstName = "studento one",
                LastName = "numberone",
                StudentId = 101,
                School = new School { Name = "School 1", Students = new HashSet<Student>() },
                GradLevel = 10,
                IsGraduated = false,
                PasswordHash = "password",
                EmailAddress = "101@schol1.com"
            },
            new Student
            {
                FirstName = "studento two",
                LastName = "number two",
                StudentId = 102,
                School = new School { Name = "School 2", Students = new HashSet<Student>() },
                GradLevel = 11,
                IsGraduated = false,
                PasswordHash = "password",
                EmailAddress = "102@schol2.com"
            },
        };
    }
}
