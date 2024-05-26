using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Data.DBSeeders;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;
using Xunit;

namespace OgrenciAidatSistemi.Tests
{
    public class PaymentServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _options;

        private readonly ILogger<PaymentServiceTests> _logger;

        private readonly IConfiguration _configuration;

        private readonly AppDbContext _dbContext;

        private readonly IServiceProvider _serviceProvider;

        private readonly PaymentService _paymentService;
        private readonly PaymentDBSeeder _paymentDBSeeder;

        private readonly HashSet<PaidPayment> _paidseedData;

        private readonly HashSet<PaymentPeriod> _paymentPeriods;

        private readonly HashSet<UnPaidPayment> _unPaidPayments;

        private readonly StudentService _studentService;

        public PaymentServiceTests()
        {
            // Use an in-memory database for testing

            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _configuration = Helpers.CreateConfiguration();

            _dbContext = new AppDbContext(_options);

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDatabase"))
                .AddScoped<StudentService>()
                .AddScoped<PaymentService>()
                .BuildServiceProvider();

            _studentService = _serviceProvider.GetRequiredService<StudentService>();

            _paymentService = _serviceProvider.GetRequiredService<PaymentService>();

            _paymentDBSeeder = new PaymentDBSeeder(
                context: _dbContext,
                configuration: _configuration,
                logger: Helpers.CreateLogger<PaymentDBSeeder>(),
                studentService: _studentService,
                randomSeed: true
            );

            _dbContext.SaveChanges();

            _paidseedData ??= [];
            _paymentPeriods = [];
            _unPaidPayments = [];

            var seedData = _paymentDBSeeder
                .GetSeedData()
                .Where(p => p.PaymentPeriod != null)
                .ToHashSet();

            // band-aid fix for student service
            // this looks terrible but it is the only way to make it work
            // id don't want to rewrite dbSeeders and re-implement dependable seeders etc.
            // seed student and make sure they have student id and email address
            // fuck i hate this

            // we copy school and first break relationship
            // after saving db reconnect it
            var schools = seedData.Select(pp => pp.Student.School).ToHashSet();
            foreach (School school in schools)
            {
                school.Students = null;
                school.WorkYears = null;
                _dbContext.Schools.Add(school);
            }

            _dbContext.SaveChanges();
            foreach (var (payment, school) in seedData.Zip(schools))
            {
                payment.Student.School = school;
                payment.Student.StudentId = _studentService.GenerateStudentId(school);
                payment.Student.EmailAddress = payment.Student.StudentId + $"@mail.school.com";
                payment.Student.ContactInfo.Email = payment.Student.EmailAddress;

                payment.PaymentPeriod.WorkYear.School = school;
            }

            _dbContext.SaveChanges();

            foreach (var seedEntity in seedData.Where(p => p is PaidPayment))
            {
                try
                {
                    if (seedEntity is PaidPayment paidPayment)
                    {
                        _paidseedData.Add(paidPayment);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while seeding paid payments");
                }
            }

            foreach (var seedEntity in seedData)
            {
                _ = _paymentPeriods.Add(seedEntity.PaymentPeriod);
            }

            // save _paymentPeriods to the database but not save _seedData payments to the database yet we will use them for testing
            foreach (var paymentPeriod in _paymentPeriods)
            {
                paymentPeriod.Payments = new HashSet<Payment>();
                if (paymentPeriod.Payments.Count > 0)
                {
                    throw new Exception("not works as expected");
                }
            }

            // create nonpaid payments for each payment period to later turn them into paid payments
            foreach (var paymentPeriod in _paymentPeriods)
            {
                var amount =
                    _paidseedData.FirstOrDefault(p => p.PaymentPeriod == paymentPeriod)?.Amount
                    ?? paymentPeriod.PerPaymentAmount;
                UnPaidPayment payment =
                    new()
                    {
                        Amount = amount,
                        PaymentPeriod = paymentPeriod,
                        Student = paymentPeriod.Student,
                    };
                try
                {
                    _dbContext.Payments.Add(payment);
                    _unPaidPayments.Add(payment);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while creating non-paid payments");
                }
            }

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task MakePayment_ValidPayment_ReturnsTrue()
        {
            foreach (var paidPayment in _paidseedData)
            {
                var paymentPeriod = paidPayment.PaymentPeriod;
                var nonPaidPayment =
                    _unPaidPayments.FirstOrDefault(p => p.PaymentPeriod == paymentPeriod)
                    ?? throw new Exception("non-paid payment not found");

                bool result = await _paymentService.MakePayment(nonPaidPayment, paidPayment);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task MakePayment_InvalidPayment_ReturnsFalse()
        {
            // create invalid payments from _paidseedData
            var invalidPayments = new HashSet<PaidPayment>();

            foreach (var paidPayment in _paidseedData)
            {
                // create a copy of paid payment and change the amount to make it invalid
                var invalidPayment = paidPayment.Copy();
                invalidPayment.Amount *= -1;
            }

            foreach (var invalidPayment in invalidPayments)
            {
                var paymentPeriod = invalidPayment.PaymentPeriod;
                var nonPaidPayment =
                    _unPaidPayments.FirstOrDefault(p => p.PaymentPeriod == paymentPeriod)
                    ?? throw new Exception("non-paid payment not found");

                bool result = await _paymentService.MakePayment(nonPaidPayment, invalidPayment);
                Assert.False(result);
            }
        }
    }
}
