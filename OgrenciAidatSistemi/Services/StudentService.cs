using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class StudentService(ILogger<UserService> logger, AppDbContext dbContext)
    {
        private static readonly int MAX_CALL_COUNT_GEN = 10;
        private readonly ILogger<UserService> _logger = logger;
        private readonly AppDbContext _dbContext = dbContext;

        // TODO: if this service not extends its functionality, it can be removed
        public string GenerateStudentId(School school, bool force = true)
        {
            Student student =
                new()
                {
                    School = school,
                    PasswordHash = "123456",
                    EmailAddress = "",
                };

            if (GenerateStudentId(_dbContext, student, force))
                return student.StudentId;
            throw new InvalidOperationException("Cannot generate unique student id.");
        }

        public static bool GenerateStudentId(
            AppDbContext context,
            Student student,
            bool force = false
        )
        {
            School? school =
                student.School
                ?? throw new InvalidOperationException("School is not set for the student.");
            int schoolId = school.Id;
            int studentCount = 0;

            if (school.Students == null)
            {
                studentCount = context.Students.Count(s =>
                    s.School != null && s.School.Name == school.Name
                );
            }
            else
                studentCount = school.Students.Count;

            var uuid = Guid.NewGuid().ToString().Replace("-", "")[..5];

            string studentId = $"{uuid}{schoolId:D2}{studentCount:D3}";

            student.StudentId = studentId;
            if (!force)
                return IsStudentIdUnique(context, studentId);

            int callCount = 0;
            while (!IsStudentIdUnique(context, studentId))
            {
                if (GenerateStudentId(context, student, false))
                    return true;
                callCount++;
                if (callCount > MAX_CALL_COUNT_GEN)
                {
                    throw new InvalidOperationException("Cannot generate unique student id.");
                }
            }
            return true;
        }

        public static bool IsStudentIdUnique(AppDbContext context, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return false;
            if (studentId.Length != 10)
                return false;
            return !context.Students.Any(s => s.StudentId == studentId);
        }
    }
}
