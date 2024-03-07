using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models
{

    public class SiteAdmin : User, IBaseDbModel
    {
        public int Id { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        public SiteAdmin(
            string Username,
            string FirstName,
            string? LastName,
            string EmailAddress,
            string PasswordHash
        )
        {
            this.Username = Username;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.EmailAddress = EmailAddress;
            this.PasswordHash = PasswordHash;
        }

        //@TODO: find a way to remove this constructor
        /// <summary>
        /// This constructor is only for DBSeeder
        /// </summary>
        public SiteAdmin()
        {
        }


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
                SiteAdmin siteAdmin = new(
                                Username,
                                FirstName: FirstName,
                                LastName: LastName,
                                EmailAddress: EmailAddress,
                                PasswordHash: SiteAdmin.ComputeHash(Password)
                            );
                return siteAdmin;
            }
        }

        public override bool CheckUsernameExists(AppDbContext dbctx)
     => dbctx.SiteAdmins.Any(admin => admin.Username == Username);

    }

}
