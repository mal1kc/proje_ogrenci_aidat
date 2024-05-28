using Bogus;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;
#pragma warning disable CS8604 // Possible null reference argument.
namespace OgrenciAidatSistemi.Data
{
    public class StudentDBSeeder(
        AppDbContext context,
        IConfiguration configuration,
        ILogger logger,
        StudentService studentService,
        int maxSeedCount = 40,
        bool randomSeed = false
    ) : DbSeeder<AppDbContext, Student>(context, configuration, logger, maxSeedCount, randomSeed)
    {
        private readonly StudentService _studentService = studentService;

        private readonly Faker faker = new("tr");

        protected override async Task SeedDataAsync()
        {
            _context.Students ??= _context.Set<Student>();

            var dbCount = await _context.Students.CountAsync();
            if (dbCount >= _maxSeedCount)
            {
                return;
            }
            foreach (var student in _seedData)
            {
                await SeedEntityAsync(student);
                if (_seedCount + dbCount >= _maxSeedCount)
                {
                    break;
                }
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task SeedRandomDataAsync()
        {
            var dbCount = await _context.Students.CountAsync();
            if (dbCount >= _maxSeedCount)
                return;

            var students = GetSeedData();
            foreach (var student in students)
            {
                await SeedEntityAsync(student);

                if (_seedCount + dbCount >= _maxSeedCount)
                    break;
            }
            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging)
            {
                var students = await _context
                    .Students.Where(s => s.CreatedAt > DateTime.UtcNow.AddMinutes(-3))
                    .ToListAsync();
                _logger.LogInformation(
                    "StudentDBSeeder: AfterSeedDataAsync students: we have {} added in last 3 minutes",
                    students.Count
                );
            }
            if (_randomSeed)
                return;
            foreach (var student in _seedData)
            {
                // check they are in the db by name , lastname and gradlevel
                // not email and studentid because they are generated randomly
                var dbStudent =
                    await _context.Students.FirstOrDefaultAsync(s =>
                        s.FirstName == student.FirstName
                        && s.LastName == student.LastName
                        && s.GradLevel == student.GradLevel
                    )
                    ?? throw new Exception(
                        $"StudentDBSeeder: AfterSeedDataAsync student: {student.FirstName} {student.LastName} {student.GradLevel} not found in db"
                    );
            }
        }

        protected override Student CreateRandomModel()
        {
            var school = new School
            {
                Name = "RandomSchool" + faker.Random.Number(100),
                Students = null
            };

            var student = new Student
            {
                School = school,
                GradLevel = faker.Random.Number(1, 12),
                IsLeftSchool = faker.Random.Bool(),
                PasswordHash = Student.ComputeHash("password"),
                EmailAddress = "temp@random.com",
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                UpdatedAt = DateTime.UtcNow,
            };
            student.ContactInfo = new ContactInfo
            {
                Email = student.EmailAddress,
                PhoneNumber = faker.Phone.PhoneNumber(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            return student;
        }

        public override IEnumerable<Student> GetSeedData()
        {
            if (randomSeed)
            {
                return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
            }
            return _seedData;
        }

        public override async Task SeedEntityAsync(Student entity)
        {
            if (await _context.Students.AnyAsync(s => s.StudentId == entity.StudentId))
            {
                return;
            }
            entity.ContactInfo = new ContactInfo
            {
                Email = entity.EmailAddress,
                PhoneNumber = "+90 555 555 55 55",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.EmailAddress = entity.StudentId + $"@mail.school.com";
            entity.ContactInfo.Email = entity.EmailAddress;

            School? assumed_sch = null;
            try
            {
                assumed_sch = await _context.Schools.FirstAsync(s =>
                    entity.School != null && s.Name == entity.School.Name
                );
            }
            catch (InvalidOperationException)
            {
                // ignored
            }

            if (_verboseLogging)
            {
                _logger.LogInformation(
                    $"StudentDBSeeder: SeedEntityAsync assumed_sch: {assumed_sch}"
                );
            }

            if (assumed_sch != null)
            {
                entity.School = assumed_sch;
            }
            else
            {
                entity.School ??= new School
                {
                    Name = "RandomSchool" + faker.Random.Number(100),
                    Students = null
                };
                entity.School.CreatedAt = DateTime.UtcNow;
                entity.School.UpdatedAt = DateTime.UtcNow;
            }

            // regenerate unique id because we have updated the school
            entity.StudentId = _studentService.GenerateStudentId(entity.School);
            entity.EmailAddress = entity.StudentId + $"@mail.school.com";
            entity.ContactInfo.Email = entity.EmailAddress;

            // check email exists in db if it does, raise an exception
            if (await _context.Users.AnyAsync(u => u.EmailAddress == entity.EmailAddress))
            {
                throw new Exception(
                    $"StudentDBSeeder: SeedEntityAsync EmailAddress: {entity.EmailAddress} already exists"
                );
            }

            await _context.Students.AddAsync(entity);
            _seedCount++;
        }

        private readonly List<Student> _seedData =
        [
            new()
            {
                FirstName = "studento one",
                LastName = "numberone",
                School = new School { Name = "School 1", Students = null },
                GradLevel = 10,
                IsLeftSchool = false,
                PasswordHash = Student.ComputeHash("password"),
                EmailAddress = "101@schol1.com"
            },
            new()
            {
                FirstName = "studento two",
                LastName = "number two",
                School = new School { Name = "School 2", Students = null },
                GradLevel = 11,
                IsLeftSchool = false,
                PasswordHash = Student.ComputeHash("password"),
                EmailAddress = "102@schol2.com"
            },
        ];
    }
}
