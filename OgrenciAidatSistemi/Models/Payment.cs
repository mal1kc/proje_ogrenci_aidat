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

        public Student Student { get; set; }
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
        public StudentView Student { get; set; }
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
        public ISet<Payment> Payments { get; set; }
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
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }
    }

    public class CreditCardPayment : Payment
    {
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CVC { get; set; }
    }

    public class CheckPayment : Payment
    {
        public string CheckNumber { get; set; }
        public string BankName { get; set; }
        public string BranchCode { get; set; }
    }

    public class DebitCardPayment : Payment
    {
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CVC { get; set; }
    }

    public class CashPayment : Payment
    {
        public string CashierName { get; set; }
        public FilePath Receipt { get; set; }
    }

    public class BankTransferPayment : Payment
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }
    }
}
