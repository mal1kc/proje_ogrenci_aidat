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

    public class PaymentPeriod : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }

        public Student? Student { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
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

        public static ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                PaymentPeriodSearchConfig.AllowedFieldsForSearch,
                PaymentPeriodSearchConfig.AllowedFieldsForSort
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
        public decimal TotalAmount { get; set; }
        public Occurrence Occurrence { get; set; }
    }

    public static class PaymentPeriodSearchConfig
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
