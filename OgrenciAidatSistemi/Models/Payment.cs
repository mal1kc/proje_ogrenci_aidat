using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace OgrenciAidatSistemi.Models
{
    public enum PaymentMethod
    {
        UnPaid, // when only not paid
        Cash,
        Bank,
        DebitCard,
        Check
    }

    public enum PaymentStatus
    {
        Paid,
        Unpaid,

        Verified,
    }

    public abstract class Payment : BaseDbModel, ISearchableModel<Payment>
    {
        public Student? Student { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentPeriod? PaymentPeriod { get; set; }
        public int PaymentPeriodId { get; set; }

        public decimal Amount { get; set; }

        public bool IsVerified => Status == PaymentStatus.Verified;

        public PaymentStatus Status { get; set; }

        public Receipt? Receipt { get; set; }

        public School? School { get; set; }

        // TODO: add subclass search config ot way to implement search config for subclasses
        public static ModelSearchConfig<Payment> SearchConfig =>
            new(
                defaultSortMethod: s => s.PaymentDate,
                sortingMethods: new()
                {
                    { "PaymentDate", static s => s.PaymentDate },
                    { "Amount", static s => (int)s.Amount },
                    { "Status", static s => s.Status },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt },
                    { "StudentId", static s => s.Student == null ? s.PaymentDate : s.Student.Id },
                    { "PaymentPeriodId", static s => s.PaymentPeriodId },
                    { "SchoolId", static s => s.School == null ? s.PaymentDate : s.School.Id },
                    { "PaymentMethod", static s => s.PaymentMethod },
                    { "ReceiptId", static s => s.Receipt == null ? s.PaymentDate : s.Receipt.Id },
                    // {"IsVerified", static s => s.IsVerified ? 1 : 0} fails to sort prob needs fix at QueryableModelHelper
                },
                searchMethods: new()
                {
                    {
                        "PaymentID",
                        static (s, searchString) =>
                            s
                                .Id.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "PaymentDate",
                        static (s, searchString) =>
                            s
                                .PaymentDate.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "Amount",
                        static (s, searchString) =>
                            s
                                .Amount.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "Status",
                        static (s, searchString) =>
                            s
                                .Status.ToString()
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
                    {
                        "PaymentMethod",
                        static (s, searchString) =>
                            s
                                .PaymentMethod.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "PaymentPeriodId",
                        static (s, searchString) =>
                            s
                                .PaymentPeriodId.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    }
                }
            );

        public Payment()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            PaymentDate = DateTime.UtcNow;
            Status = PaymentStatus.Unpaid; // default status
            PaymentMethod = PaymentMethod.UnPaid;
        }

        public abstract PaymentView ToView(bool ignoreBidirectNav = false);
    }

    // only for distinguishing between paid and non-paid payment
    public abstract class PaidPayment : Payment
    {
        public PaidPayment()
        {
            Status = PaymentStatus.Paid;
        }

        public abstract PaidPayment Copy();
    }

    public class UnPaidPayment : Payment
    {
        public UnPaidPayment()
        {
            PaymentMethod = PaymentMethod.UnPaid;
        }

        public override PaymentView ToView(bool ignoreBidirectNav = false)
        {
            return new UnPaidPaymentView
            {
                Id = Id,
                PaymentMethod = PaymentMethod,
                Student = ignoreBidirectNav ? null : Student?.ToView(ignoreBidirectNav: true),
                PaymentDate = PaymentDate,
                Amount = Amount,
                Status = Status,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }

    public class BankPayment : PaidPayment
    {
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BranchCode { get; set; }

        public required string IBAN { get; set; }

        public BankPayment()
        {
            PaymentMethod = PaymentMethod.Bank;
        }

        public override PaymentView ToView(bool ignoreBidirectNav = false)
        {
            return new BankPaymentView
            {
                Id = Id,
                PaymentMethod = PaymentMethod,
                Student = ignoreBidirectNav ? null : Student?.ToView(ignoreBidirectNav: true),
                PaymentDate = PaymentDate,
                Amount = Amount,
                Status = Status,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                BankName = BankName,
                AccountNumber = AccountNumber,
                BranchCode = BranchCode,
                IBAN = IBAN
            };
        }

        public override PaidPayment Copy()
        {
            return new BankPayment
            {
                BankName = BankName,
                AccountNumber = AccountNumber,
                BranchCode = BranchCode,
                IBAN = IBAN
            };
        }
    }

    public class CheckPayment : PaidPayment
    {
        public required string CheckNumber { get; set; }
        public required string BankName { get; set; }
        public required string BranchCode { get; set; }

        public CheckPayment()
        {
            PaymentMethod = PaymentMethod.Check;
        }

        public override PaymentView ToView(bool ignoreBidirectNav = false)
        {
            return new CheckPaymentView
            {
                Id = Id,
                PaymentMethod = PaymentMethod,
                Student = ignoreBidirectNav ? null : Student?.ToView(ignoreBidirectNav: true),
                PaymentDate = PaymentDate,
                Amount = Amount,
                Status = Status,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                CheckNumber = CheckNumber,
                BankName = BankName,
                BranchCode = BranchCode
            };
        }

        public override PaidPayment Copy()
        {
            return new CheckPayment
            {
                CheckNumber = CheckNumber,
                BankName = BankName,
                BranchCode = BranchCode
            };
        }
    }

    public class DebitCardPayment : PaidPayment
    {
        public required string CardNumber { get; set; }
        public required string CardHolderName { get; set; }
        public required string ExpiryDate { get; set; }
        public required char[] CVC { get; set; }

        public DebitCardPayment()
        {
            PaymentMethod = PaymentMethod.DebitCard;
        }

        public override PaymentView ToView(bool ignoreBidirectNav = false)
        {
            return new DebitCardPaymentView
            {
                Id = Id,
                PaymentMethod = PaymentMethod,

                Student = ignoreBidirectNav ? null : Student?.ToView(ignoreBidirectNav: true),
                PaymentDate = PaymentDate,
                Amount = Amount,
                Status = Status,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                CardNumber = CardNumber,
                CardHolderName = CardHolderName,
                ExpiryDate = ExpiryDate,
                CVC = CVC
            };
        }

        public override PaidPayment Copy()
        {
            return new DebitCardPayment
            {
                CardNumber = CardNumber,
                CardHolderName = CardHolderName,
                ExpiryDate = ExpiryDate,
                CVC = CVC
            };
        }
    }

    public class CashPayment : PaidPayment
    {
        public required string CashierName { get; set; }
        public required string ReceiptNumber { get; set; }
        public required DateTime ReceiptDate { get; set; }
        public required string ReceiptIssuer { get; set; }

        public CashPayment()
        {
            PaymentMethod = PaymentMethod.Cash;
        }

        public override PaymentView ToView(bool ignoreBidirectNav = false)
        {
            return new CashPaymentView
            {
                Id = Id,
                PaymentMethod = PaymentMethod,
                Student = ignoreBidirectNav ? null : Student?.ToView(ignoreBidirectNav: true),
                PaymentDate = PaymentDate,
                Amount = Amount,
                Status = Status,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                CashierName = CashierName,
                ReceiptNumber = ReceiptNumber,
                ReceiptDate = ReceiptDate,
                ReceiptIssuer = ReceiptIssuer
            };
        }

        public override PaidPayment Copy()
        {
            return new CashPayment
            {
                CashierName = CashierName,
                ReceiptNumber = ReceiptNumber,
                ReceiptDate = ReceiptDate,
                ReceiptIssuer = ReceiptIssuer
            };
        }
    }

    public static class PaymentMethodExtensions
    {
        public static string ToFriendlyString(this PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Cash => "Nakit",
                PaymentMethod.Bank => "Banka",
                PaymentMethod.DebitCard => "Banka/Kredi Kartı",
                PaymentMethod.Check => "Çek",
                _ => "Bilinmiyor"
            };
        }
    }
}
