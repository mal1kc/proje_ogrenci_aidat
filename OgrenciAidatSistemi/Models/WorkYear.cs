using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class WorkYear : BaseDbModel, ISearchableModel<WorkYear>
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public ISet<PaymentPeriod>? PaymentPeriods { get; set; }
        public School? School { get; set; }
        public static ModelSearchConfig<WorkYear> SearchConfig =>
            new(
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "StartDate", static s => s.StartDate },
                    { "EndDate", static s => s.EndDate },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt }
                },
                searchMethods: new()
                {
                    {
                        "Id",
                        static (s, searchString) =>
                            s
                                .Id.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "StartDate",
                        static (s, searchString) =>
                            s
                                .StartDate.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "EndDate",
                        static (s, searchString) =>
                            s
                                .EndDate.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "CreatedAt",
                        static (s, searchString) =>
                            s
                                .CreatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "UpdatedAt",
                        static (s, searchString) =>
                            s
                                .UpdatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    }
                }
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
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public ISet<PaymentPeriodView>? PaymentPeriods { get; set; }
        public SchoolView? School { get; set; }

        public int? SchoolId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public decimal? TotalAmount { get; set; }
    }
}
