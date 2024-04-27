using OgrenciAidatSistemi.Models.Interfaces;
namespace OgrenciAidatSistemi.Models
{
    public class WorkYear : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ISet<PaymentPeriode>? PaymentPeriods { get; set; }
        public ISet<School>? Schools { get; set; }
        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
            WorkYearSearchConfig.AllowedFieldsForSearch,
            WorkYearSearchConfig.AllowedFieldsForSort
            );
    }

    public class WorkYearView : IBaseDbModelView
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ISet<PaymentPeriodeView>? PaymentPeriods { get; set; }
        public ISet<SchoolView>? Schools { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public static class WorkYearSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "id",
            "StartDate",
            "EndDate"
        };
        public static readonly string[] AllowedFieldsForSort = new string[]
        {
            "id",
            "StartDate",
            "EndDate"
        };
    }
}
