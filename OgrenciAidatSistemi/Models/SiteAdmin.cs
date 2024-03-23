using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models
{
    public class SiteAdmin : User
    {
        public int SiteAdminId { get; set; }
        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

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
                        emailAddress: EmailAdress,
                        passwordHash: SiteAdmin.ComputeHash(Password)
                    );
                return siteAdmin;
            }
        }

        public override bool CheckUsernameExists(AppDbContext dbctx) =>
            dbctx.SiteAdmins.Any(admin => admin.Username == Username);
    }
}
