using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models
{
    public class SchoolAdmin : User
    {
        public int SchoolId { get; set; }
        public School _School { get; set; }

        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

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
        public SchoolView School { get; set; }

        public override bool? CheckUsernameExists(AppDbContext dbctx)
        {
            throw new NotImplementedException();
        }
    }
}
