using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("SiteAdmins")]
    public class SiteAdmin : User, ISearchableModel
    {
        public string Username { get; set; }
        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        public static ModelSearchConfig SearchConfig =>
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
            Role = UserRole.SiteAdmin;
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            PasswordHash = passwordHash;
        }

        /// <summary>
        /// This constructor is only for DBSeeder
        /// </summary>
        public SiteAdmin()
        {
            Role = UserRole.SiteAdmin;
        }

        public static string ComputeHash(string rawData)
        {
            return ComputeHash(rawData, Constants.AdminPasswordSalt);
        }

        public SiteAdminView ToView()
        {
            return new SiteAdminView
            {
                Id = Id,
                Username = Username,
                FirstName = FirstName,
                LastName = LastName,
                EmailAddress = EmailAddress,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
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

        public override UserViewValidationResult ValidateFieldsSignIn()
        {
            if (!CheckNamesLenght())
                return UserViewValidationResult.InvalidName;
            return base.ValidateFieldsSignIn();
        }
    }

    public static class SiteAdminSearchConfig
    {
        public static string[] AllowedFieldsForSearch =>
            new string[] { "Username", "FirstName", "LastName", "EmailAddress" };
        public static string[] AllowedFieldsForSort =>
            new string[] { "Id", "CreatedAt", "UpdatedAt" }
                .Concat(AllowedFieldsForSearch)
                .ToArray();
    }
}
