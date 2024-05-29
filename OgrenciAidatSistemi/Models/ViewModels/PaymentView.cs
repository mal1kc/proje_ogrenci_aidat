using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public abstract class PaymentView : IBaseDbModelView
    {
        public int Id { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public StudentView? Student { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public PaymentStatus? Status { get; set; }
    }

    internal class UnPaidPaymentView : PaymentView { }

    public class BankPaymentView : PaymentView
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BranchCode { get; set; }
        public string? IBAN { get; set; }
    }

    public class CheckPaymentView : PaymentView
    {
        public string? CheckNumber { get; set; }
        public string? BankName { get; set; }
        public string? BranchCode { get; set; }

        public CheckPaymentView() => PaymentMethod = PaymentMethod.Check;
    }

    public class DebitCardPaymentView : PaymentView
    {
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryDate { get; set; }
        public char[]? CVC { get; set; }

        public DebitCardPaymentView()
        {
            PaymentMethod = PaymentMethod.CreditCard;
        }
    }

    public class CashPaymentView : PaymentView
    {
        public string? CashierName { get; set; }
        public string? ReceiptNumber { get; set; }
        public DateTime? ReceiptDate { get; set; }

        public CashPaymentView() => PaymentMethod = PaymentMethod.Cash;
    }
}
