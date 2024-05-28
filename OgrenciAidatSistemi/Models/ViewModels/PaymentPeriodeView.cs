using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class PaymentPeriodView : IBaseDbModelView
    {
        public int Id { get; set; }

        public StudentView? Student { get; set; }

        public ISet<PaymentView>? Payments { get; set; }

        public WorkYearView? WorkYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public decimal PerPaymentAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Occurrence Occurrence { get; set; }

        public int? WorkYearId { get; set; }
        public int? StudentId { get; set; }

        public bool IsExhausted => EndDate < DateOnly.FromDateTime(DateTime.UtcNow);

        public bool IsActive =>
            StartDate <= DateOnly.FromDateTime(DateTime.UtcNow)
            && EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
