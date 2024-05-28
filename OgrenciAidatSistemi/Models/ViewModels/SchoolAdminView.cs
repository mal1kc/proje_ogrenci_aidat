using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models.ViewModels
{
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
