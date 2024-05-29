using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class SchoolAdminView : UserView
    {
        public SchoolView? School { get; set; }
        public int? SchoolId { get; set; }

        public ContactInfoView? ContactInfo { get; set; }

        // if the current date is between the start and end date of a work year, return that first work year

        public WorkYearView? CurrentWorkYear =>
            School?.WorkYears?.FirstOrDefault(static wy =>
                wy.StartDate <= DateOnly.FromDateTime(DateTime.Now)
                && wy.EndDate >= DateOnly.FromDateTime(DateTime.Now)
            );

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
