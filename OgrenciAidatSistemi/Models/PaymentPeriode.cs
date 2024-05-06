using System.Text.Json;
using System.Text.Json.Serialization;
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

    public class PaymentPeriode : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }

        public Student Student { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }

        // public required School School { get; set; } is already linked via Payment and WorkYear
        public required ISet<Payment> Payments { get; set; }
        public required WorkYear WorkYear { get; set; }
        public Occurrence Occurrence { get; set; }

        public PaymentPeriode()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Occurrence = Occurrence.Monthly; // default occurrence
        }

        public static ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                PaymentPeriodeSearchConfig.AllowedFieldsForSearch,
                PaymentPeriodeSearchConfig.AllowedFieldsForSort
            );
    }

    public class PaymentPeriodeView
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public Occurrence Occurence { get; set; }

        public PaymentPeriodeView(PaymentPeriode periode)
        {
            throw new NotImplementedException("Not implemented yet");
        }
    }

    public static class PaymentPeriodeSearchConfig
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
