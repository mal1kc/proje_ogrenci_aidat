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

            var students = GetSeedData(true);
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
                    .Students.Where(s => s.CreatedAt > DateTime.Now.AddMinutes(-3))
                    .ToListAsync();
                Console.WriteLine(
                    $"StudentDBSeeder: AfterSeedDataAsync students: \n we have {students.Count} added in last 3 minutes"
                );
            }

            foreach (var student in _seedData)
            {
                // check they are in the db by name , lastname and gradlevel
                // not email and studentid because they are generated randomly
                var dbStudent = await _context.Students.FirstOrDefaultAsync(s =>
                    s.FirstName == student.FirstName
                    && s.LastName == student.LastName
                    && s.GradLevel == student.GradLevel
                );

                if (dbStudent == null)
                {
                    throw new Exception(
                        $"StudentDBSeeder: AfterSeedDataAsync student: {student.FirstName} {student.LastName} {student.GradLevel} not found in db"
                    );
                }
            }
        }

        protected override Student CreateRandomModel()
        {
            var school = new School
            {
                Name = "RandomSchool" + random.Next(100),
                Students = new HashSet<Student>()
            };

            var student = new Student
            {
                School = school,
                GradLevel = random.Next(1, 13),
                IsGraduated = random.Next(2) == 0, // Generate a random graduation status
                PasswordHash = Student.ComputeHash("password"),
                EmailAddress = "temp@random.com"
            };
            return student;
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
            entity.ContactInfo = new ContactInfo
            {
                Email = entity.EmailAddress,
                PhoneNumber = "+90 555 555 55 55",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;
            entity.GenerateUniqueId(_context);
            entity.EmailAddress = entity.StudentId + $"@mail.school.com";
            entity.ContactInfo.Email = entity.EmailAddress;

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
                entity.School.Students.Add(entity);
                entity.School.Id = _context.Schools.Count() + 1;
                entity.School.CreatedAt = DateTime.Now;
                entity.School.UpdatedAt = DateTime.Now;
            }

            // regenerate unique id because we have updated the school
            entity.GenerateUniqueId(_context);
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

        private readonly List<Student> _seedData = new List<Student>
        {
            new Student
            {
                FirstName = "studento one",
                LastName = "numberone",
                School = new School { Name = "School 1", Students = new HashSet<Student>() },
                GradLevel = 10,
                IsGraduated = false,
                PasswordHash = Student.ComputeHash("password"),
                EmailAddress = "101@schol1.com"
            },
            new Student
            {
                FirstName = "studento two",
                LastName = "number two",
                School = new School { Name = "School 2", Students = new HashSet<Student>() },
                GradLevel = 11,
                IsGraduated = false,
                PasswordHash = Student.ComputeHash("password"),
                EmailAddress = "102@schol2.com"
            },
        };
    }
}
