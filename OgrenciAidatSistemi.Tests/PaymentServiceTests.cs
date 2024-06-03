using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Data.DBSeeders;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Tests
{
    public class PaymentServiceTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _dbContext;

        private readonly FileService _fileService;
        private readonly PaymentService _paymentService;
        private readonly StudentService _studentService;
        private readonly PaymentDBSeeder _paymentDBSeeder;

        private HashSet<PaidPayment> _paidSeedData;
        private HashSet<PaymentPeriod> _paymentPeriods;
        private HashSet<UnPaidPayment> _unPaidPayments;

        public PaymentServiceTests()
        {
            // Use an in-memory database for testing
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var mockConfiguration = new Mock<IConfiguration>();

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(m => m.WebRootPath).Returns("testpath"); // Set this to a suitable value for your tests

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDatabase"))
                .AddScoped<StudentService>()
                .AddScoped<PaymentService>()
                .AddScoped<FileService>()
                .AddSingleton<IWebHostEnvironment>(mockEnvironment.Object)
                .AddSingleton<IConfiguration>(mockConfiguration.Object)
                .BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            _studentService = _serviceProvider.GetRequiredService<StudentService>();
            _fileService = _serviceProvider.GetRequiredService<FileService>();
            _paymentService = _serviceProvider.GetRequiredService<PaymentService>();

            _paymentDBSeeder = new PaymentDBSeeder(
                context: _dbContext,
                configuration: Helpers.CreateConfiguration(),
                logger: Helpers.CreateLogger<PaymentDBSeeder>(),
                paymentService: _paymentService,
                studentService: _studentService,
                randomSeed: true
            );

            InitializeSeedData();
        }

        private void InitializeSeedData()
        {
            // Initial seeding
            _paidSeedData = [];
            _paymentPeriods = [];
            _unPaidPayments = [];

            var seedData = _paymentDBSeeder
                .GetSeedData()
                .Where(p => p.PaymentPeriod != null)
                .ToHashSet();

            // Separate handling of schools and students
            var schools = seedData.Select(pp => pp.Student?.School).Distinct().ToHashSet();
            foreach (var school in schools)
            {
                if (school != null)
                {
                    school.Students = null;
                    school.WorkYears?.AsParallel().ForAll(wy => wy.PaymentPeriods = null);
                    _dbContext.Schools.Add(school);
                }
            }

            _dbContext.SaveChanges();

            // Band-aid fix for student service
            // This looks terrible but it is the only way to make it work
            // I don't want to rewrite dbSeeders and re-implement dependable seeders etc.
            // Seed student and make sure they have student ID and email address
            foreach (var payment in seedData)
            {
                var student = payment.Student;
                var school = schools.First(s => s?.Id == student?.School?.Id);
                if (student != null)
                {
                    student.School = school;
                    if (school == null)
                        continue;
                    student.StudentId = _studentService.GenerateStudentId(school);
                    student.EmailAddress = $"{student.StudentId}@mail.school.com";
                    if (student.ContactInfo != null)
                        student.ContactInfo.Email = student.EmailAddress;
                    if (payment.PaymentPeriod != null && payment.PaymentPeriod.WorkYear != null)
                        payment.PaymentPeriod.WorkYear.School = school;

                    _dbContext.Students.Add(student);
                }
                // Note: Do not save payments to DB here, only handle them in tests
            }

            _dbContext.SaveChanges();

            foreach (var seedEntity in seedData.OfType<PaidPayment>())
            {
                _paidSeedData.Add(seedEntity);
            }

            _paymentPeriods = seedData
                .Where(p => p.PaymentPeriod != null)
                .Select(p => p.PaymentPeriod)
                .ToHashSet();

            foreach (var paymentPeriod in _paymentPeriods)
            {
                var unpaidPayment = new UnPaidPayment
                {
                    Amount =
                        _paidSeedData.FirstOrDefault(p => p.PaymentPeriod == paymentPeriod)?.Amount
                        ?? paymentPeriod.PerPaymentAmount,
                    PaymentPeriod = paymentPeriod,
                    Student = paymentPeriod.Student,
                };
                _unPaidPayments.Add(unpaidPayment);
            }
        }

        [Fact]
        public async Task MakePayment_ValidPayment_ReturnsTrue()
        {
            foreach (var paidPayment in _paidSeedData)
            {
                var nonPaidPayment =
                    _unPaidPayments.FirstOrDefault(p =>
                        p.PaymentPeriod == paidPayment.PaymentPeriod
                    ) ?? throw new Exception("non-paid payment not found");

                // Save the non-paid payment to the DB for this test case
                _dbContext.Payments.Add(nonPaidPayment);
                _dbContext.SaveChanges();

                bool result = await _paymentService.MakePayment(nonPaidPayment, paidPayment);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task MakePayment_ValidPayment_PaymentIsSaved()
        {
            foreach (var paidPayment in _paidSeedData)
            {
                var nonPaidPayment =
                    _unPaidPayments.FirstOrDefault(p =>
                        p.PaymentPeriod == paidPayment.PaymentPeriod
                    ) ?? throw new Exception("non-paid payment not found");

                // Save the non-paid payment to the DB for this test case
                _dbContext.Payments.Add(nonPaidPayment);
                _dbContext.SaveChanges();

                bool result = await _paymentService.MakePayment(nonPaidPayment, paidPayment);
                Assert.True(result);

                var savedPayment = await _dbContext
                    .Payments.OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync(p => p.PaymentPeriod == paidPayment.PaymentPeriod);

                Assert.NotNull(savedPayment);
            }
        }

        [Fact]
        public async Task MakePayment_ValidPayment_PaymentIsPaid()
        {
            var paidMethods = new HashSet<PaymentMethod>
            {
                PaymentMethod.Cash,
                PaymentMethod.Bank,
                PaymentMethod.CreditCard,
                PaymentMethod.Check,
            };

            foreach (var paidPayment in _paidSeedData)
            {
                var nonPaidPayment =
                    _unPaidPayments.FirstOrDefault(p =>
                        p.PaymentPeriod == paidPayment.PaymentPeriod
                    ) ?? throw new Exception("non-paid payment not found");

                // Save the non-paid payment to the DB for this test case
                _dbContext.Payments.Add(nonPaidPayment);
                _dbContext.SaveChanges();

                bool result = await _paymentService.MakePayment(nonPaidPayment, paidPayment);
                Assert.True(result);

                var savedPayment = await _dbContext
                    .Payments.OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync(p => p.PaymentPeriod == paidPayment.PaymentPeriod);

                Assert.NotNull(savedPayment);
                Assert.Contains(savedPayment.PaymentMethod, paidMethods);
            }
        }

        [Fact]
        public async Task MakePayment_InvalidPayment_ReturnsFalse()
        {
            // create invalid payments from _paidseedData
            var invalidPayments = new HashSet<PaidPayment>();

            foreach (var paidPayment in _paidSeedData)
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

        [Fact]
        public async Task MakePayment_InvalidPayment_PaymentIsNotSaved()
        {
            // create invalid payments from _paidseedData
            var invalidPayments = new HashSet<PaidPayment>();

            foreach (var paidPayment in _paidSeedData)
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

                await _paymentService.MakePayment(nonPaidPayment, invalidPayment);

                var savedPayment = await _dbContext
                    .Payments.OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync(p => p.PaymentPeriod == paymentPeriod);

                Assert.Null(savedPayment);
            }
        }

        [Fact]
        public async Task MakePayment_ValidPayment_ReceiptGenerated()
        {
            foreach (var paidPayment in _paidSeedData)
            {
                var nonPaidPayment =
                    _unPaidPayments.FirstOrDefault(p =>
                        p.PaymentPeriod == paidPayment.PaymentPeriod
                    ) ?? throw new Exception("non-paid payment not found");

                // Save the non-paid payment to the DB for this test case
                _dbContext.Payments.Add(nonPaidPayment);
                _dbContext.SaveChanges();

                bool result = await _paymentService.MakePayment(nonPaidPayment, paidPayment);
                Assert.True(result);

                var savedPayment = await _dbContext
                    .Payments.OrderByDescending(p => p.PaymentDate)
                    .Where(p => p.Status == PaymentStatus.Paid && p.Receipt != null)
                    .FirstOrDefaultAsync(p => p.PaymentPeriod == paidPayment.PaymentPeriod);

                Assert.NotNull(savedPayment);

                var receipt = await _dbContext.Receipts.FirstOrDefaultAsync(r =>
                    r.Payment == savedPayment
                );
                Console.WriteLine($"this receipt has ctype = {receipt?.ContentType}");
                Assert.NotNull(receipt);
                Assert.True(receipt.ContentType == "text/plain"); // This is a generated file

                var receiptContent = await _fileService.DownloadFileAsync(receipt);
                Assert.NotNull(receiptContent);
            }
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
