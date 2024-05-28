using Coravel.Invocable;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Invokables
{
    public class PaymentCreatorInvocable(PaymentService paymentService) : IInvocable
    {
        private readonly PaymentService _paymentService = paymentService;

        public Task Invoke()
        {
            return _paymentService.CreatePayments();
        }
    }
}
