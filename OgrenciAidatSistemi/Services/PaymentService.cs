using System.Text;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class PaymentService(
        AppDbContext context,
        ILogger<PaymentService> logger,
        FileService fileService
    )
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;

        private readonly int _loggerInterval = 100;

        private readonly FileService _fileService = fileService;

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
            int counter = 0;
            foreach (var paymentPeriod in paymentPeriods)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // start timer

                // Check if a payment for this period already exists
                var existingPayment = paymentPeriod.Payments?.FirstOrDefault(p =>
                    (
                        paymentPeriod.Occurrence == Occurrence.Monthly
                            && p.PaymentDate.Month == DateTime.UtcNow.Month
                        || paymentPeriod.Occurrence == Occurrence.Daily
                            && p.PaymentDate.Date == DateTime.UtcNow.Date
                        || paymentPeriod.Occurrence == Occurrence.Weekly
                            && p.PaymentDate.GetWeekOfYear() == DateTime.UtcNow.GetWeekOfYear()
                        || paymentPeriod.Occurrence == Occurrence.Yearly
                            && p.PaymentDate.Year == DateTime.UtcNow.Year
                    )
                );
                if (existingPayment == null)
                {
                    UnPaidPayment payment =
                        new()
                        {
                            Amount = paymentPeriod.PerPaymentAmount,
                            PaymentPeriod = paymentPeriod,
                            Student = paymentPeriod.Student,
                            School = paymentPeriod.Student.School,
                        };
                    _context.Payments.Add(payment);
                }

                stopwatch.Stop(); // stop timer

                counter++;
                if (counter % _loggerInterval == 0)
                {
                    _logger.LogInformation(
                        $"Created 100 payments in {stopwatch.ElapsedMilliseconds} ms"
                    );
                }
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
            if (newPayment.Amount <= 0)
            {
                return false;
            }

            // Remove the unpaid payment
            _context.Payments.Remove(payment);

            // Update new payment's properties from non-paid payment
            newPayment.Student = payment.Student;
            newPayment.PaymentPeriod = payment.PaymentPeriod;
            newPayment.School = payment.School;
            newPayment.PaymentDate = DateTime.UtcNow;
            newPayment.Status = PaymentStatus.Paid;
            newPayment.UpdatedAt = DateTime.UtcNow;

            // Add the new payment
            _context.Payments.Add(newPayment);

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();

                var receipt = await GenerateReceipt(newPayment);
                if (receipt != null)
                {
                    newPayment.Receipt = receipt;
                    _context.Receipts.Add(receipt);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError("Error while generating receipt");
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while making payment");
                return false;
            }
        }

        public async Task<Receipt?> GenerateReceipt(PaidPayment payment)
        {
            // Create a string builder to build the receipt
            var receiptData = new StringBuilder();

            // Add the payment details to the receipt
            receiptData.AppendLine($"Payment ID: {payment.Id}");
            receiptData.AppendLine($"Amount: {payment.Amount}");
            receiptData.AppendLine($"Payment Date: {payment.PaymentDate}");
            receiptData.AppendLine(
                $"Student: {payment.Student?.FirstName} {payment.Student?.LastName} ({payment.Student?.StudentId})"
            );
            var school =
                payment.School != null
                    ? payment.Student?.School
                    : null ?? payment.PaymentPeriod?.WorkYear?.School;
            receiptData.AppendLine($"School: {school?.Name} ({school?.Id})");

            receiptData.AppendLine(
                $"Payment Period: {payment.PaymentPeriod?.StartDate} - {payment.PaymentPeriod?.EndDate}"
            );
            receiptData.AppendLine(
                $"Payment Period Per Payment Amount: {payment.PaymentPeriod?.PerPaymentAmount}"
            );
            receiptData.AppendLine(
                $"Payment Period Occurrence: {payment.PaymentPeriod?.Occurrence}"
            );
            receiptData.AppendLine(
                $"Payment Period Work Year: {payment.PaymentPeriod?.WorkYear?.StartDate} - {payment?.PaymentPeriod?.WorkYear?.EndDate}"
            );

            // Add the receipt data to the receipt object


            // Generate a unique file name for the receipt
            var fileName = $"Receipt_{payment?.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";

            // Write the receipt to a file
            try
            {
                var receiptFilePath = await _fileService.WriteFileAsync(
                    fileName,
                    receiptData.ToString(),
                    "text/plain",
                    payment.Student,
                    "/receipts"
                );
                var receipt = Receipt.FromFilePath(receiptFilePath);
                receipt.FileHash = receipt.ComputeHash();
                receipt.Payment = payment;
                return receipt;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while generating receipt");
                return null;
            }
        }
    }
}
