using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class SiteAdminView : UserView
    {
        public string Username { get; set; }

        public override bool CheckUserExists(AppDbContext dbctx)
        {
            if (dbctx.SiteAdmins == null)
            {
                throw new Exception("SiteAdmins table is null");
            }
            return dbctx.SiteAdmins.Any(admin =>
                admin.Username == Username || admin.EmailAddress == EmailAddress
            );
        }

        public override bool CheckEmailAddressExists(AppDbContext dbctx)
        {
            if (dbctx.SiteAdmins == null)
            {
                throw new Exception("SiteAdmins table is null");
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
}
