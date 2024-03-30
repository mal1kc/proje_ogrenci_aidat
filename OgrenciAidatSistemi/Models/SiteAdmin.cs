using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class SiteAdmin : User, ISearchableModel
    {
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

        //@TODO: find a way to remove this constructor
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
        public SiteAdmin ToAdmin
        {
            get
            {
                SiteAdmin siteAdmin =
                    new(
                        Username,
                        firstName: FirstName,
                        lastName: LastName,
                        emailAddress: EmailAddress,
                        passwordHash: SiteAdmin.ComputeHash(Password)
                    );
                return siteAdmin;
            }
        }

        public override bool? CheckUsernameExists(AppDbContext dbctx)
        {
            return dbctx.SiteAdmins.Any(admin => admin.Username == Username);
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
