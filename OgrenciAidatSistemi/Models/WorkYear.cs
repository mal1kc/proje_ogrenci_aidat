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
        public ISet<PaymentPeriod>? PaymentPeriods { get; set; }
        public School? School { get; set; }
        public static ModelSearchConfig SearchConfig =>
            new(
                WorkYearSearchConfig.AllowedFieldsForSearch,
                WorkYearSearchConfig.AllowedFieldsForSort
            );

        public decimal TotalAmount()
        {
            return PaymentPeriods?.Sum(pp => pp.TotalAmount) ?? 0;
        }

        public WorkYearView ToView(bool ignoreBidirectNav = false)
        {
            return new WorkYearView
            {
                Id = Id,
                StartDate = StartDate,
                EndDate = EndDate,
                PaymentPeriods = ignoreBidirectNav
                    ? null
                    : PaymentPeriods?.Select(pp => pp.ToView(true)).ToHashSet(),
                School = ignoreBidirectNav ? null : School?.ToView(true),
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                TotalAmount = PaymentPeriods?.Sum(pp => pp.TotalAmount) ?? null
            };
        }
    }

    public class WorkYearView : IBaseDbModelView
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ISet<PaymentPeriodView>? PaymentPeriods { get; set; }
        public SchoolView? School { get; set; }

        public int? SchoolId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public decimal? TotalAmount { get; set; }
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
