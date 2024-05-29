using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    [Table("SiteAdmins")]
    public class SiteAdmin : User, ISearchableModel<SiteAdmin>
    {
        public string Username { get; set; }

        public static ModelSearchConfig<SiteAdmin> SearchConfig =>
            new(
                defaultSortMethod: s => s.CreatedAt,
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

        public override SiteAdminView ToView(bool ignoreBidirectNav = false)
        {
            return new SiteAdminView
            {
                Role = Role,
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
}
