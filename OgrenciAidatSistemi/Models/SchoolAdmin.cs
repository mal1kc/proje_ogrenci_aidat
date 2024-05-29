using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    [Table("SchoolAdmins")]
    public class SchoolAdmin : User, ISearchableModel<SchoolAdmin>
    {
        public School School { get; set; }

        public ContactInfo ContactInfo { get; set; }

        public static ModelSearchConfig<SchoolAdmin> SearchConfig =>
            new(
                defaultSortMethod: s => s.CreatedAt,
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "FirstName", static s => s.FirstName },
                    { "LastName", static s => s.LastName ?? "" },
                    { "EmailAddress", static s => s.EmailAddress },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt },
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
                        "FirstName",
                        static (s, searchString) =>
                            s.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "LastName",
                        static (s, searchString) =>
                            (s.LastName ?? "").Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            )
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
                    },
                }
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

        public override SchoolAdminView ToView(bool ignoreBidirectNav = false)
        {
            return new()
            {
                Role = Role,
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                School = ignoreBidirectNav ? null : School?.ToView(ignoreBidirectNav: true),
                EmailAddress = EmailAddress,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                ContactInfo = ContactInfo?.ToView() ?? new ContactInfoView { Email = EmailAddress }
            };
        }
    }
}
