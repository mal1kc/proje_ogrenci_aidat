using System.ComponentModel.DataAnnotations.Schema;
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
                    { "LastName", static s => s.LastName ?? "" },
                    { "StudentId", static s => s.StudentId },
                    // Sort by school by name
                    { "School", static s => s.School != null ? s.School.Name : "" },
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
                            s.LastName?.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                            ?? false
                    },
                    {
                        "StudentId",
                        static (s, searchString) =>
                            s.StudentId.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "School",
                        static (s, searchString) =>
                            s.School?.Name.Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            ) ?? false
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

        public override UserViewValidationResult ValidateFieldsSignIn()
        {
            if (!IsStudentIdValid(StudentId))
                return UserViewValidationResult.InvalidName;
            if (string.IsNullOrEmpty(Password))
                return UserViewValidationResult.PasswordEmpty;
            return UserViewValidationResult.FieldsAreValid;
        }

        public static bool IsStudentIdValid(string studentId)
        {
            if (studentId.Length != 10)
                return false;
            if (!studentId.Substring(5, 2).All(char.IsDigit))
                return false;
            if (!studentId.Substring(7, 3).All(char.IsDigit))
                return false;
            return true;
        }
    }
}
