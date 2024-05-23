using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class StudentService(ILogger<UserService> logger, AppDbContext dbContext)
    {
        private readonly ILogger<UserService> _logger = logger;
        private readonly AppDbContext _dbContext = dbContext;

        public string GenerateStudentId(School school)
        {
            if (school == null)
            {
                throw new InvalidOperationException("School is not set for the student.");
            }

            int schoolId = school.Id;
            int studentCount = 0;

            if (school.Students == null)
            {
                studentCount = _dbContext.Students.Count(s =>
                    s.School != null && s.School.Name == school.Name
                );
            }
            else
                studentCount = school.Students.Count;

            var uuid = Guid.NewGuid().ToString().Replace("-", "")[..5];

            string studentId = $"{uuid}{schoolId:D2}{studentCount:D3}";
            if (studentId.Length != 10)
            {
                throw new InvalidOperationException("Generated student id is not 10 digits.");
            }
            return studentId;
        }

        public bool IsStudentIdUnique(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new InvalidOperationException("Student id is not set.");
            }
            return !_dbContext.Students.Any(s => s.StudentId == studentId);
        }
    }
}
