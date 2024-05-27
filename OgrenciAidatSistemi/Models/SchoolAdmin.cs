using System.ComponentModel.DataAnnotations.Schema;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    [Table("SchoolAdmins")]
    public class SchoolAdmin : User, ISearchableModel<SchoolAdmin>
    {
        public School School { get; set; }

        public ContactInfo ContactInfo { get; set; }

        public static ModelSearchConfig<SchoolAdmin> SearchConfig =>
            new(
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

        public SchoolAdminView ToView(bool ignoreBidirectNav = false)
        {
            return new SchoolAdminView()
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                School = ignoreBidirectNav ? null : this.School.ToView(ignoreBidirectNav: true),
                EmailAddress = this.EmailAddress,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
                ContactInfo =
                    this.ContactInfo?.ToView() ?? new ContactInfoView { Email = this.EmailAddress }
            };
        }
    }

    public class SchoolAdminView : UserView
    {
        public SchoolView? School { get; set; }
        public int? SchoolId { get; set; }

        public ContactInfoView? ContactInfo { get; set; }

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
}
