using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("Students")]
    public class Student : User, ISearchableModel
    {
        public string StudentId { get; set; }
        public required School School { get; set; }
        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<PaymentPeriode>? PaymentPeriods { get; set; }
        public ICollection<Grade>? Grades { get; set; }
        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        public ContactInfo? ContactInfo { get; set; }

        public Student()
        {
            Role = UserRole.Student;
        }

        public StudentView ToView(bool ignoreBidirectNav = false)
        {
            return new StudentView()
            {
                Id = this.Id,
                StudentId = this.StudentId,
                FirstName = this.FirstName,
                LastName = this.LastName,
                School = ignoreBidirectNav ? null : this.School.ToView(ignoreBidirectNav: true),
                GradLevel = this.GradLevel,
                IsGraduated = this.IsGraduated,
                EmailAddress = this.EmailAddress,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
                Payments = ignoreBidirectNav ? null : this.Payments?.ToHashSet(),
                Grades = ignoreBidirectNav ? null : this.Grades?.ToHashSet(),
                ContactInfo = ContactInfo?.ToView(),
            };
        }

        public static ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                StudentSearchConfig.AllowedFieldsForSearch,
                StudentSearchConfig.AllowedFieldsForSort
            );

        private string GenStudentId(AppDbContext dbctx)
        {
            // generate student id by using school id and student count and unique id
            // it needs to be unique

            if (this.School == null)
            {
                throw new InvalidOperationException("School is not set for the student.");
            }

            // Get the school id

            int schoolId = this.School.Id;
            int studentCount = 0;

            if (this.School.Students == null)
            {
                studentCount = dbctx
                    .Schools.Include(s => s.Students)
                    .Where(s => s.Id == schoolId)
                    .FirstOrDefault()
                    .Students.Count;
            }

            // Get the count of students in the school
            studentCount = this.School.Students.Count;

            var uuid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);

            // Generate the student id
            string studentId = $"{uuid}{schoolId:D2}{studentCount:D3}";
            if (studentId.Length != 10)
            {
                throw new InvalidOperationException("Generated student id is not 10 digits.");
            }
            return studentId;
        }

        public void GenerateUniqueId(AppDbContext dbctx, bool recall = false)
        {
            var studentId = GenStudentId(dbctx);
            if (string.IsNullOrEmpty(studentId) && StudentId != studentId)
                throw new InvalidOperationException(
                    "Student id is already set and it is different from the generated one."
                );

            if (
                dbctx.Students.Where(s => s.StudentId == studentId).FirstOrDefault() != null
                && !recall
            )
                GenerateUniqueId(dbctx, true);
            StudentId = studentId;
        }
    }

    public class StudentView : UserView
    {
        public int SchoolId { get; set; }
        public SchoolView? School { get; set; }
        public string StudentId { get; set; }

        public ContactInfoView ContactInfo { get; set; }

        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }

        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Grade>? Grades { get; set; }

        public ICollection<PaymentPeriodeView>? PaymentPeriods { get; set; }

        public override bool CheckUserExists(AppDbContext dbctx)
        {
            if (dbctx.Students.Where(s => s.StudentId == this.StudentId).FirstOrDefault() != null)
            {
                return true;
            }

            return false;
        }

        public override bool CheckEmailAddressExists(AppDbContext dbctx)
        {
            if (
                dbctx.Users.Where(u => u.EmailAddress == this.EmailAddress).FirstOrDefault() != null
            )
            {
                return true;
            }
            return false;
        }
    }

    public static class StudentSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "Id",
            "StudentId",
            "Username",
            "FirstName",
            "LastName",
            "EmailAddress",
            "GradLevel",
            "IsGraduated"
        };

        public static readonly string[] AllowedFieldsForSort = new string[]
        {
            "Id",
            "StudentId",
            "Username",
            "FirstName",
            "LastName",
            "EmailAddress",
            "GradLevel",
            "IsGraduated",
            "CreatedAt",
            "UpdatedAt"
        };
    }
}
