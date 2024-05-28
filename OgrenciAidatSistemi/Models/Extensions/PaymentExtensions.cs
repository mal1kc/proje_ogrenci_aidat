using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models.Extensions
{
    public static class PaymentStatusExtensions
    {
        public static string ToFriendlyString(this PaymentStatus paymentStatus)
        {
            return paymentStatus switch
            {
                PaymentStatus.Paid => "Ödenmiş",
                PaymentStatus.Unpaid => "Ödenmemiş",
                PaymentStatus.Verified => "Doğrulanmış",
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
            },
            { PaymentMethod.UnPaid, Array.Empty<string>() }
        };

        public static string[] GetFields(PaymentMethod paymentMethod) => Fields[paymentMethod];

        public static bool ValidateFields()
        {
            var paymentTypeDict = new Dictionary<PaymentMethod, Type>
            {
                { PaymentMethod.Bank, typeof(BankPayment) },
                { PaymentMethod.Check, typeof(CheckPayment) },
                { PaymentMethod.DebitCard, typeof(DebitCardPayment) },
                { PaymentMethod.Cash, typeof(CashPayment) },
                { PaymentMethod.UnPaid, typeof(UnPaidPayment) }
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
