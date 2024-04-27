using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{

    [Table("Students")]
    public class Student : User, ISearchableModel
    {
        public int StudentId { get; set; }
        public School School { get; set; }
        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }
        public ISet<Payment>? Payments { get; set; }
        public ISet<PaymentPeriode>? PaymentPeriods { get; set; }
        public ISet<Grade>? Grades { get; set; }
        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        public ContactInfo ContactInfo { get; set; }

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
            };
        }


        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                StudentSearchConfig.AllowedFieldsForSearch,
                StudentSearchConfig.AllowedFieldsForSort
            );

    }

    public class StudentView : UserView
    {

        public int SchoolId { get; set; }
        public SchoolView? School { get; set; }
        public int StudentId { get; set; }
        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }

        public override bool CheckUserExists(AppDbContext dbctx)
        {
            throw new NotImplementedException();
        }

        public override bool CheckEmailAddressExists(AppDbContext dbctx)
        {
            throw new NotImplementedException();
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
