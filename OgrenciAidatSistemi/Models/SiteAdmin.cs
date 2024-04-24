using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("SiteAdmins")]
    public class SiteAdmin : User, ISearchableModel
    {
        override public UserRole Role => UserRole.SiteAdmin;
        public string Username { get; set; }
        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        // ignore this field in serialization
        [System.Text.Json.Serialization.JsonIgnore]
        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                SiteAdminSearchConfig.AllowedFieldsForSearch,
                SiteAdminSearchConfig.AllowedFieldsForSort
            );

        public SiteAdmin(
            string username,
            string firstName,
            string? lastName,
            string emailAddress,
            string passwordHash
        )
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            PasswordHash = passwordHash;
        }

        /// <summary>
        /// This constructor is only for DBSeeder
        /// </summary>
        public SiteAdmin() { }

        public static string ComputeHash(string rawData)
        {
            return ComputeHash(rawData, Constants.AdminPasswordSalt);
        }
    }

    public class SiteAdminView : UserView
    {
        public string Username { get; set; }

        public override bool CheckUserExists(AppDbContext dbctx)
        {
            if (dbctx.SiteAdmins == null)
            {
                throw new System.Exception("SiteAdmins table is null");
            }
            return dbctx.SiteAdmins.Any(admin =>
                admin.Username == Username || admin.EmailAddress == EmailAddress
            );
        }

        public override bool CheckEmailAddressExists(AppDbContext dbctx)
        {
            if (dbctx.SiteAdmins == null)
            {
                throw new System.Exception("SiteAdmins table is null");
            }
            return dbctx.SiteAdmins.Any(admin => admin.EmailAddress == EmailAddress);
        }
    }

    public static class SiteAdminSearchConfig
    {
        public static string[] AllowedFieldsForSearch =>
            new string[] { "Username", "FirstName", "LastName", "EmailAddress" };
        public static string[] AllowedFieldsForSort =>
            new string[] { "CreatedAt", "UpdatedAt" }
                .Concat(AllowedFieldsForSearch)
                .ToArray();
    }
}
