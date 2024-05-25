using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;

        public async Task CreatePayments()
        {
            var currentTime = DateOnly.FromDateTime(DateTime.Today);
            List<PaymentPeriod> paymentPeriods = await _context
                .PaymentPeriods.Include(pp => pp.WorkYear)
                .Include(pp => pp.Student)
                .Where(pp => pp.WorkYear != null && pp.WorkYear.EndDate > currentTime)
                .Where(pp => pp.StartDate <= currentTime)
                .Where(pp => pp.Student != null && pp.Student.IsLeftSchool == false)
                .ToListAsync();

            // for each student create a payment for each payment period
            foreach (var paymentPeriod in paymentPeriods)
            {
                UnPaidPayment payment =
                    new()
                    {
                        Amount = paymentPeriod.PerPaymentAmount,
                        PaymentPeriod = paymentPeriod,
                        Student = paymentPeriod.Student,
                    };
                _context.Payments.Add(payment);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating payments");
            }
        }

        public async Task<bool> MakePayment(UnPaidPayment payment, PaidPayment newPayment)
        {
            // if new payment is not valid, return false
            // TODO: make more checks for new payment
            if (newPayment.Amount <= 0)
            {
                return false;
            }
            // Remove the non-paid payment
            _context.Payments.Remove(payment);

            // Add the paid payment
            _context.Payments.Add(newPayment);

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while making payment");
                return false;
            }
        }
    }
}
