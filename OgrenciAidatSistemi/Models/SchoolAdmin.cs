using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class SchoolAdmin : User, ISearchableModel
    {
        public int SchoolId { get; set; }
        public School _School { get; set; }

        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                SchoolAdminSearchConfig.AllowedFieldsForSearch,
                SchoolAdminSearchConfig.AllowedFieldsForSort
            );

        public SchoolAdmin(
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

        // <summary>
        // This constructor is only for DBSeeder
        // </summary>
        public SchoolAdmin() { }
    }

    public class SchoolAdminView : UserView
    {
        public int Id { get; set; }
        /* public SchoolView School { get; set; } */
        public int SchoolId { get; set; }

        public override bool CheckUsernameExists(AppDbContext dbctx)
        {
            if (dbctx.SchoolAdmins == null)
            {
                throw new System.Exception("SchoolAdmins table is null");
            }
            return dbctx.SchoolAdmins.Any(s => s.Username == Username);
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
            "username",
            "firstName",
            "lastName",
            "emailAddress"
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
