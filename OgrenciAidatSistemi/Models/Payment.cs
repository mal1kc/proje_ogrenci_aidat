using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace OgrenciAidatSistemi.Models
{
    public enum PaymentMethod
    {
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
        Partial
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
                sortingMethods: new()
                {
                    { "PaymentDate", static s => s.PaymentDate },
                    { "Amount", static s => s.Amount },
                    { "Status", static s => s.Status },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt }
                },
                searchMethods: new()
                {
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
                    }
                }
            );

        public Payment()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            PaymentDate = DateTime.Now;
            Status = PaymentStatus.Unpaid; // default status
        }

        public abstract PaymentView ToView(bool ignoreBidirectNav = false);
    }

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
    }

    public class BankPaymentView : PaymentView
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }
        public string IBAN { get; set; }
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
    }

    public class CheckPaymentView : PaymentView
    {
        public string CheckNumber { get; set; }
        public string BankName { get; set; }
        public string BranchCode { get; set; }

        public CheckPaymentView()
        {
            PaymentMethod = PaymentMethod.Check;
        }
    }

    public class DebitCardPayment : Payment
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
    }

    public class DebitCardPaymentView : PaymentView
    {
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public char[] CVC { get; set; }

        public DebitCardPaymentView()
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
    }

    public class CashPaymentView : PaymentView
    {
        public string CashierName { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReceiptIssuer { get; set; }
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

    public static class PaymentStatusExtensions
    {
        public static string ToFriendlyString(this PaymentStatus paymentStatus)
        {
            return paymentStatus switch
            {
                PaymentStatus.Paid => "Ödenmiş",
                PaymentStatus.Unpaid => "Ödenmemiş",
                PaymentStatus.Verified => "Doğrulanmış",
                PaymentStatus.Partial => "Kısmi",
                _ => "Bilinmiyor"
            };
        }
    }

    public static class PaymentExtensions
    {
        public static PaymentView ToView(this Payment payment, bool ignoreBidirectNav = false)
        {
            return payment switch
            {
                BankPayment bankPayment => bankPayment.ToView(ignoreBidirectNav),
                CheckPayment checkPayment => checkPayment.ToView(ignoreBidirectNav),
                DebitCardPayment debitCardPayment => debitCardPayment.ToView(ignoreBidirectNav),
                CashPayment cashPayment => cashPayment.ToView(ignoreBidirectNav),
                _ => throw new NotImplementedException()
            };
        }
    }

    public static class PaymentMethodSpecificFields
    {
        public static Dictionary<PaymentMethod, string[]> Fields = new Dictionary<
            PaymentMethod,
            string[]
        >
        {
            {
                PaymentMethod.Bank,
                new string[] { "BankName", "AccountNumber", "BranchCode", "IBAN" }
            },
            { PaymentMethod.Check, new string[] { "CheckNumber", "BankName", "BranchCode" } },
            {
                PaymentMethod.DebitCard,
                new string[] { "CardNumber", "CardHolderName", "ExpiryDate", "CVC" }
            },
            {
                PaymentMethod.Cash,
                new string[] { "CashierName", "ReceiptNumber", "ReceiptDate", "ReceiptIssuer" }
            }
        };

        public static string[] GetFields(PaymentMethod paymentMethod) => Fields[paymentMethod];

        public static bool ValidateFields()
        {
            var paymentTypeDict = new Dictionary<PaymentMethod, Type>
            {
                { PaymentMethod.Bank, typeof(BankPayment) },
                { PaymentMethod.Check, typeof(CheckPayment) },
                { PaymentMethod.DebitCard, typeof(DebitCardPayment) },
                { PaymentMethod.Cash, typeof(CashPayment) }
            };

            var truthFlags = new List<bool>();

            foreach (var (paymentMethod, type) in paymentTypeDict)
            {
                var fields = GetFields(paymentMethod);
                var properties = type.GetProperties();

                var truthFlag = fields.All(field => properties.Any(prop => prop.Name == field));
                truthFlags.Add(truthFlag);
            }

            return truthFlags.All(flag => flag);
        }
    }
}
