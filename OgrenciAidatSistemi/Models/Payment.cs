using System.Text.Json;
using System.Text.Json.Serialization;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public enum PaymentMethod
    {
        Cash,
        Bank,
        CreditCard,
        DebitCard,
        Check
    }

    public enum PaymentStatus
    {
        Paid,
        Unpaid,

        Verified,
        Partial
    }

    public abstract class Payment : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public required Student? Student { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentPeriode? PaymentPeriode { get; set; }
        public int PaymentPeriodId { get; set; }

        public decimal Amount { get; set; }

        public bool IsVerified => Status == PaymentStatus.Verified;

        public PaymentStatus Status { get; set; }

        public FilePath? Receipt { get; set; }

        public static ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                PaymentSearchConfig.AllowedFieldsForSearch,
                PaymentSearchConfig.AllowedFieldsForSort
            );

        public Payment()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            PaymentDate = DateTime.Now;
            Status = PaymentStatus.Unpaid; // default status
        }

        // to json
        public string ToJson()
        {
            // has cyclic reference like Student -> School -> WorkYear -> PaymentPeriod -> Payment -> Student

            return JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve }
            );
        }
    }

    public class PaymentView : IBaseDbModelView
    {
        public int Id { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public required StudentView Student { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BankPayment : Payment
    {
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BranchCode { get; set; }

        public required string IBAN { get; set; }

        public BankPayment()
        {
            PaymentMethod = PaymentMethod.Bank;
        }
    }

    public class CreditCardPayment : Payment
    {
        public required string CardNumber { get; set; }
        public required string CardHolderName { get; set; }
        public required string ExpiryDate { get; set; }
        public required string CVC { get; set; }

        public CreditCardPayment()
        {
            PaymentMethod = PaymentMethod.CreditCard;
        }
    }

    public class CheckPayment : Payment
    {
        public required string CheckNumber { get; set; }
        public required string BankName { get; set; }
        public required string BranchCode { get; set; }

        public CheckPayment()
        {
            PaymentMethod = PaymentMethod.Check;
        }
    }

    public class DebitCardPayment : Payment
    {
        public required string CardNumber { get; set; }
        public required string CardHolderName { get; set; }
        public required string ExpiryDate { get; set; }
        public required string CVC { get; set; }

        public DebitCardPayment()
        {
            PaymentMethod = PaymentMethod.DebitCard;
        }
    }

    public class CashPayment : Payment
    {
        public required string CashierName { get; set; }
        public required string ReceiptNumber { get; set; }
        public required DateTime ReceiptDate { get; set; }
        public required string ReceiptIssuer { get; set; }

        public CashPayment()
        {
            PaymentMethod = PaymentMethod.Cash;
        }
    }

    public static class PaymentSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "PaymentMethod",
            "PaymentDate",
            "Amount",
            "CreatedAt",
            "UpdatedAt"
        };
        public static readonly string[] AllowedFieldsForSort = new string[]
        {
            "PaymentMethod",
            "PaymentDate",
            "Amount",
            "CreatedAt",
            "UpdatedAt",
            "Status"
        };
    }
}
