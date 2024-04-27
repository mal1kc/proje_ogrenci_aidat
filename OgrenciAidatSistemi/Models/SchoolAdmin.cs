using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("SchoolAdmins")]
    public class SchoolAdmin : User, ISearchableModel
    {
        public School School { get; set; }

        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        public ContactInfo ContactInfo { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                SchoolAdminSearchConfig.AllowedFieldsForSearch,
                SchoolAdminSearchConfig.AllowedFieldsForSort
            );

        public SchoolAdmin(
            string firstName,
            string? lastName,
            string emailAddress,
            string passwordHash
        )
        {
            Role = UserRole.SchoolAdmin;
            EmailAddress = emailAddress;
            FirstName = firstName;
            LastName = lastName;
            PasswordHash = passwordHash;
        }

        // <summary>
        // This constructor is only for DBSeeder
        // </summary>
        public SchoolAdmin()
        {
            Role = UserRole.SchoolAdmin;
        }

        public SchoolAdminView ToView(bool ignoreBidirectNav = false)
        {
            return new SchoolAdminView()
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                SchoolView = ignoreBidirectNav ? null : this.School.ToView(ignoreBidirectNav: true),
                EmailAddress = this.EmailAddress,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
            };
        }
    }

    public class SchoolAdminView : UserView
    {
        public SchoolView? SchoolView { get; set; }
        public int SchoolId { get; set; }


        public override bool CheckUserExists(AppDbContext dbctx)
        {
            if (dbctx.SchoolAdmins == null)
            {
                throw new System.Exception("SchoolAdmins table is null");
            }
            return dbctx.SchoolAdmins.Any(s => s.EmailAddress == EmailAddress);
        }

        public override bool CheckEmailAddressExists(AppDbContext dbctx)
        {
            if (dbctx.SchoolAdmins == null)
            {
                throw new System.Exception("SchoolAdmins table is null");
            }
            return dbctx.SchoolAdmins.Any(s => s.EmailAddress == EmailAddress);
        }
    }

    public static class SchoolAdminSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "Id",
            "FirstName",
            "LastName",
            "EmailAddress"
        };
        public static readonly string[] AllowedFieldsForSort = new string[]
        {
            "CreatedAt",
            "UpdatedAt",
        }
            .Concat(AllowedFieldsForSearch)
            .ToArray();
    }
}
