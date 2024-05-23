using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("Students")]
    public class Student : User, ISearchableModel<Student>
    {
        public string StudentId { get; set; }
        public School? School { get; set; }
        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<PaymentPeriod>? PaymentPeriods { get; set; }
        public ICollection<Grade>? Grades { get; set; }

        public ContactInfo? ContactInfo { get; set; }

        public Student()
        {
            Role = UserRole.Student;
        }

        public static ModelSearchConfig<Student> SearchConfig =>
            new(
                sortingMethods: new()
                {
                    { "FirstName", static s => s.FirstName },
                    { "LastName", static s => s.LastName },
                    // Sort by school by name
                    { "School", static s => s.School.Name },
                    // Sort by gradlevel
                    { "GradLevel", static s => s.GradLevel },
                    // Sort by email address
                    { "EmailAddress", static s => s.EmailAddress }
                },
                searchMethods: new()
                {
                    {
                        "FirstName",
                        static (s, searchString) =>
                            s.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "LastName",
                        static (s, searchString) =>
                            s.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "StudentId",
                        static (s, searchString) =>
                            s.StudentId.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "School",
                        static (s, searchString) =>
                            s.School.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "GradLevel",
                        static (s, searchString) =>
                            s
                                .GradLevel.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "EmailAddress",
                        static (s, searchString) =>
                            s.EmailAddress.Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            )
                    },
                    {
                        "CreatedAt",
                        static (s, searchString) =>
                            s
                                .CreatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "UpdatedAt",
                        static (s, searchString) =>
                            s
                                .UpdatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    }
                }
            );

        public StudentView ToView(bool ignoreBidirectNav = false)
        {
            return new StudentView()
            {
                Id = Id,
                StudentId = StudentId,
                FirstName = FirstName,
                LastName = LastName,
                School = ignoreBidirectNav ? null : School?.ToView(ignoreBidirectNav: true),
                GradLevel = GradLevel,
                IsGraduated = IsGraduated,
                EmailAddress = EmailAddress,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Payments = ignoreBidirectNav ? null : Payments?.ToHashSet(),
                Grades = ignoreBidirectNav ? null : Grades?.ToHashSet(),
                ContactInfo = ignoreBidirectNav
                    ? null
                    : ContactInfo?.ToView(ignoreBidirectNav: true),
            };
        }

        private string GenStudentId(AppDbContext dbctx)
        {
            // generate student id by using school id and student count and unique id
            // it needs to be unique

            if (this.School == null)
            {
                throw new InvalidOperationException("School is not set for the student.");
            }

            // Get the school id
            int schoolId = School.Id;
            int studentCount = 0;

            if (School.Students == null)
                studentCount = dbctx.Students.Count(s => s.School.Name == School.Name);
            // Name is unique like Id but id is autoincremented so it is not reliable
            else
                studentCount = School.Students.Count;

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

        public ICollection<PaymentPeriodView>? PaymentPeriods { get; set; }

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
}
