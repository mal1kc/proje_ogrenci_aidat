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
        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        public Student()
        {
            Role = UserRole.Student;
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
