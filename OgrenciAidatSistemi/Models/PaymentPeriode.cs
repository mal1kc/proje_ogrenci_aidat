using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    public enum Occurrence
    {
        Monthly,
        Yearly,
        Weekly,
        Daily
    }

    public class PaymentPeriod : BaseDbModel, ISearchableModel<PaymentPeriod>
    {
        public Student? Student { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal PerPaymentAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<Payment>? Payments { get; set; }

        public required WorkYear? WorkYear { get; set; }
        public Occurrence Occurrence { get; set; }

        public bool IsExhausted => EndDate < DateOnly.FromDateTime(DateTime.UtcNow);

        public bool IsActive =>
            StartDate <= DateOnly.FromDateTime(DateTime.UtcNow)
            && EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);

        public PaymentPeriod()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Occurrence = Occurrence.Monthly; // default occurrence
        }

        public static ModelSearchConfig<PaymentPeriod> SearchConfig =>
            new(
                defaultSortMethod: s => s.CreatedAt,
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "StartDate", static s => s.StartDate },
                    { "EndDate", static s => s.EndDate },
                    { "TotalAmount", static s => (int)s.TotalAmount },
                    { "PerPaymentAmount", static s => (int)s.PerPaymentAmount },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt },
                    { "StudentId", static s => s.Student == null ? "" : s.Student.StudentId },
                    { "WorkYear", static s => s.WorkYear == null ? "" : s.WorkYear.Id },
                    { "Occurrence", static s => s.Occurrence }
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
                    // search by year and month not complete date
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
                    },
                    // search amount as bigger than or equal to
                    {
                        "TotalAmount",
                        static (s, searchString) =>
                            s
                                .TotalAmount.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "PerPaymentAmount",
                        static (s, searchString) =>
                            s
                                .PerPaymentAmount.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                }
            );

        public PaymentPeriodView ToView(bool ignoreBidirectNav = false)
        {
            return new PaymentPeriodView
            {
                Id = Id,

                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                StartDate = StartDate,
                EndDate = EndDate,
                TotalAmount = TotalAmount,
                Occurrence = Occurrence,
                Student = ignoreBidirectNav ? null : Student?.ToView(true),
                Payments = ignoreBidirectNav
                    ? null
                    : Payments?.Select(p => p.ToView(true)).ToHashSet(),
                WorkYear = ignoreBidirectNav ? null : WorkYear?.ToView(true),
                PerPaymentAmount = PerPaymentAmount,
            };
        }
    }
}
