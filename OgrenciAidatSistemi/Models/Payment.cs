using System.Text.Json;
using System.Text.Json.Serialization;
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

    public abstract class Payment : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Student? Student { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentPeriod? PaymentPeriod { get; set; }
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
