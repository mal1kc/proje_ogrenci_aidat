using System.ComponentModel.DataAnnotations;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class PaymentCreateView
    {
        public static Dictionary<PaymentMethod, HashSet<string>> RequiredFields { get; } =
            new Dictionary<PaymentMethod, HashSet<string>>
            {
                {
                    PaymentMethod.Bank,
                    new HashSet<string> { "BankName", "AccountNumber", "IBAN", "BranchCode" }
                },
                {
                    PaymentMethod.Check,
                    new HashSet<string> { "CheckNumber", "BankName" }
                },
                {
                    PaymentMethod.CreditCard,
                    new HashSet<string>
                    {
                        "CardNumber",
                        "CardHolderName",
                        "ExpiryDate",
                        "CVV",
                        "BillingAddress"
                    }
                },
                {
                    PaymentMethod.Cash,
                    new HashSet<string>
                    {
                        "CashierName",
                        "ReceiptNumber",
                        "ReceiptDate",
                        "ReceiptTime",
                        "Location"
                    }
                }
            };
        public int Id { get; set; }
        public int PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public IFormFile? Receipt { get; set; }

        public decimal Amount { get; set; }

        // Bank Payment fields
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BranchCode { get; set; }
        public string? IBAN { get; set; }

        // Check Payment fields
        public string? CheckNumber { get; set; }

        // Credit Card Payment fields
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CVV { get; set; }

        // Cash Payment fields
        public string? CashierName { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? ReceiptDate { get; set; }
        public string? ReceiptTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentAmount <= 0)
            {
                yield return new ValidationResult(
                    "Payment amount must be greater than 0",
                    [nameof(PaymentAmount)]
                );
            }
            if (PaymentMethod == PaymentMethod.UnPaid)
            {
                yield return new ValidationResult(
                    "Payment method must be selected",
                    [nameof(PaymentMethod)]
                );
            }
            if (Receipt == null)
            {
                yield return new ValidationResult("Receipt is required", [nameof(Receipt)]);
            }

            var requiredFields = RequiredFields[PaymentMethod];
            foreach (var field in requiredFields)
            {
                var value = GetType().GetProperty(field)?.GetValue(this);
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    yield return new ValidationResult(
                        $"{field} is required for {PaymentMethod} payment",
                        [field]
                    );
                }
            }
        }

        public bool IsValid()
        {
            return Validate(new ValidationContext(this)).Any();
        }

        public PaidPayment ToAppropriatePayment()
        {
            foreach (var field in RequiredFields[PaymentMethod])
            {
                if (GetType().GetProperty(field)?.GetValue(this) == null)
                {
                    throw new ValidationException($"Required fields are not set {field}");
                }
            }

            return PaymentMethod switch
            {
                PaymentMethod.Bank
                    => new BankPayment
                    {
                        Id = Id,
                        Amount = Amount,
                        BankName = BankName,
                        AccountNumber = AccountNumber,
                        BranchCode = BranchCode,
                        IBAN = IBAN,
                    },
                PaymentMethod.Check
                    => new CheckPayment
                    {
                        Id = Id,
                        Amount = Amount,
                        BranchCode = BranchCode,
                        CheckNumber = CheckNumber,
                        BankName = BankName,
                    },
                PaymentMethod.CreditCard
                    => new CreditCardPayment
                    {
                        Id = Id,
                        Amount = Amount,
                        CardNumber = CardNumber,
                        CardHolderName = CardHolderName,
                        ExpiryDate = ExpiryDate,
                        CVC = [.. CVV],
                    },
                PaymentMethod.Cash
                    => new CashPayment
                    {
                        Id = Id,
                        Amount = Amount,
                        CashierName = CashierName,
                        ReceiptNumber = ReceiptNumber,
                        ReceiptDate = DateTime.Parse(ReceiptDate),
                    },
                _ => throw new InvalidOperationException("Invalid payment method")
            };
        }

        public UnPaidPayment ToUnPaidPayment()
        {
            return new UnPaidPayment { Amount = Amount, PaymentDate = PaymentDate, };
        }

        public static PaymentCreateView FromUnPaidPayment(UnPaidPayment payment)
        {
            return new PaymentCreateView
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
            };
        }
    }
}
