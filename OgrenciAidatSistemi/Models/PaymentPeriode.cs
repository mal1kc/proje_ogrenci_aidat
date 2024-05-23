using OgrenciAidatSistemi.Models.Interfaces;

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

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PerPaymentAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public required ISet<Payment> Payments { get; set; }

        public required WorkYear? WorkYear { get; set; }
        public Occurrence Occurrence { get; set; }

        public PaymentPeriod()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Occurrence = Occurrence.Monthly; // default occurrence
        }

        public static ModelSearchConfig<PaymentPeriod> SearchConfig =>
            new(
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "StartDate", static s => s.StartDate },
                    { "EndDate", static s => s.EndDate },
                    { "TotalAmount", static s => s.TotalAmount },
                    { "PerPaymentAmount", static s => s.PerPaymentAmount },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt },
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

    public class PaymentPeriodView : IBaseDbModelView
    {
        public int Id { get; set; }

        public StudentView? Student { get; set; }

        public ISet<PaymentView>? Payments { get; set; }

        public WorkYearView? WorkYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal PerPaymentAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Occurrence Occurrence { get; set; }

        public int? WorkYearId { get; set; }
        public int? StudentId { get; set; }
    }
}
