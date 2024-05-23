using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("SiteAdmins")]
    public class SiteAdmin : User, ISearchableModel<SiteAdmin>
    {
        public string Username { get; set; }

        public static ModelSearchConfig<SiteAdmin> SearchConfig =>
            new(
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "Username", static s => s.Username },
                    { "FirstName", static s => s.FirstName },
                    { "LastName", static s => s.LastName ?? "" },
                    { "EmailAddress", static s => s.EmailAddress },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt }
                },
                searchMethods: new()
                {
                    {
                        "Id",
                        static (s, searchString) =>
                            s
                                .Id.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "Username",
                        static (s, searchString) =>
                            s.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "FirstName",
                        static (s, searchString) =>
                            s.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "LastName",
                        static (s, searchString) =>
                            s.LastName != null
                            && s.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "EmailAddress",
                        static (s, searchString) =>
                            s.EmailAddress.Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            )
                    },
                    // search by year and month not complete date
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
