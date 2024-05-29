using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    [Table("Students")]
    public class Student : User, ISearchableModel<Student>
    {
        public string StudentId { get; set; }
        public School? School { get; set; }
        public int GradLevel { get; set; }
        public bool IsLeftSchool { get; set; }

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
                defaultSortMethod: s => s.CreatedAt,
                sortingMethods: new()
                {
                    { "FirstName", static s => s.FirstName },
                    { "LastName", static s => s.LastName ?? "" },
                    { "StudentId", static s => s.StudentId },
                    { "School", static s => s.School != null ? s.School.Name : "" },
                    { "GradLevel", static s => s.GradLevel },
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

        public override StudentView ToView(bool ignoreBidirectNav = false)
        {
            return new StudentView()
            {
                Role = Role,
                Id = Id,
                StudentId = StudentId,
                FirstName = FirstName,
                LastName = LastName,
                School = ignoreBidirectNav ? null : School?.ToView(ignoreBidirectNav: true),
                GradLevel = GradLevel,
                IsLeftSchool = IsLeftSchool,
                EmailAddress = EmailAddress,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Payments = ignoreBidirectNav
                    ? null
                    : Payments
                        ?.ToHashSet()
                        .Select(p => p.ToView(ignoreBidirectNav: true))
                        .ToHashSet(),
                Grades = ignoreBidirectNav ? null : Grades?.ToHashSet(),
                ContactInfo = ignoreBidirectNav
                    ? null
                    : ContactInfo?.ToView(ignoreBidirectNav: true),
                PaymentPeriods = ignoreBidirectNav
                    ? null
                    : PaymentPeriods
                        ?.ToHashSet()
                        .Select(pp => pp.ToView(ignoreBidirectNav: true))
                        .ToHashSet()
            };
        }
    }
}
