using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public enum PaymentMethod
    {
        Cash,
        BankTransfer,
        CreditCard,
        DebitCard,
        Check
    }

    public abstract class Payment : IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public required Student Student { get; set; }
        public int StudentId { get; set; }

        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentPeriode? PaymentPeriode { get; set; }
        public int PaymentPeriodId { get; set; }

        public decimal Amount { get; set; }
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

    public class PaymentPeriode : IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public required ISet<Payment> Payments { get; set; }
        public Occurence Occurence { get; set; }
    }

    public enum Occurence
    {
        Monthly,
        Yearly,
        Weekly,
        Daily
    }

    public class PaymentPeriodeView
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public Occurence Occurence { get; set; }
    }

    public class BankPayment : Payment
    {
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BranchCode { get; set; }
    }

    public class CreditCardPayment : Payment
    {
        public required string CardNumber { get; set; }
        public required string CardHolderName { get; set; }
        public required string ExpiryDate { get; set; }
        public required string CVC { get; set; }
    }

    public class CheckPayment : Payment
    {
        public required string CheckNumber { get; set; }
        public required string BankName { get; set; }
        public required string BranchCode { get; set; }
    }

    public class DebitCardPayment : Payment
    {
        public required string CardNumber { get; set; }
        public required string CardHolderName { get; set; }
        public required string ExpiryDate { get; set; }
        public required string CVC { get; set; }
    }

    public class CashPayment : Payment
    {
        public required string CashierName { get; set; }
        public required FilePath Receipt { get; set; }
    }

    public class BankTransferPayment : Payment
    {
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BranchCode { get; set; }
    }
}
