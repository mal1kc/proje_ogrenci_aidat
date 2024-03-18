using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models
{
    public class SchoolAdmin : User
{
    public int SchoolId { get; set; }
    public School School { get; set; }

        public override DateTime createdAt { get; set; }
        public override DateTime updatedAt { get; set; }
}


    public class SchoolAdminView : UserView
    {
        public int Id { get; set; }
        public SchoolView School { get; set; }

        public override bool CheckUsernameExists(AppDbContext dbctx)
        {
            throw new NotImplementedException();
        }
    }
}
