using System.Text.Json;
using Bogus;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Data.DBSeeders
{
    // this dbseeder is has no default data to seed , it will use random data default
    public class PaymentDBSeeder(
        AppDbContext context,
        IConfiguration configuration,
        ILogger logger,
        StudentService studentService,
        int maxSeedCount = 100,
        bool randomSeed = true
    ) : DbSeeder<AppDbContext, Payment>(context, configuration, logger, maxSeedCount, randomSeed)
    {
        private readonly StudentService _studentService = studentService;

        private readonly Faker faker = new("tr");

        public override IEnumerable<Payment> GetSeedData()
        {
            return Enumerable.Range(0, 10).Select(i => CreateRandomModel());
        }

        protected override Payment CreateRandomModel()
        {
            var paymentMethod = (PaymentMethod)
                faker.Random.Number(0, Enum.GetNames(typeof(PaymentMethod)).Length - 1);
            ;

            var student = new Student
            {
                FirstName = faker.Name.FirstName(),
                School = new School
                {
                    Name = "RandomSchool" + faker.Random.Number(1, 100),
                    Students = null
                },
                GradLevel = faker.Random.Number(1, 12),
                IsGraduated = faker.Random.Number(2) == 0, // Generate a random graduation status
                PasswordHash = Student.ComputeHash("password"), // dont overthink it
                EmailAddress = "temp@somemail.com"
            };

            student.ContactInfo = new ContactInfo
            {
                Email = student.EmailAddress,
                PhoneNumber = "+90 555 555 55 55",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            student.CreatedAt = DateTime.Now;
            student.UpdatedAt = DateTime.Now;
            student.School.CreatedAt = DateTime.Now;
            student.School.UpdatedAt = DateTime.Now;
            student.StudentId = _studentService.GenerateStudentId(student.School);
            student.EmailAddress = student.StudentId + $"@mail.school.com";
            student.ContactInfo.Email = student.EmailAddress;

            var school = student.School;
            PaymentPeriod paymentperiod = new PaymentPeriod
            {
                Payments = new HashSet<Payment>(),
                Student = student,
                WorkYear = new WorkYear
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now + TimeSpan.FromDays(180), // 6 months
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            Payment payment = paymentMethod switch
            {
                PaymentMethod.Cash
                    => new CashPayment
                    {
                        Student = student,
                        CashierName = "cashier 1",
                        ReceiptNumber = faker.Random.Number(1000, 9999).ToString(),
                        ReceiptIssuer = "issuer 1",
                        ReceiptDate = DateTime.UtcNow,
                        PaymentPeriod = paymentperiod,
                    },
                PaymentMethod.Bank
                    => new BankPayment
                    {
                        Student = student,
                        PaymentPeriod = paymentperiod,
                        BankName = "Bank1",
                        AccountNumber = faker.Random.Number(1000, 9999).ToString(),
                        BranchCode = faker.Random.Number(1000, 9999).ToString(),
                        // iban length is 26
                        IBAN = genIban(),
                    },
                PaymentMethod.DebitCard
                    => new DebitCardPayment
                    {
                        Student = student,
                        PaymentPeriod = paymentperiod,
                        CardNumber = "1234 5678 1234 5678",
                        CardHolderName = "cardholder 1",
                        ExpiryDate = DateTime.UtcNow.ToString(),
                        CVC = [.. faker.Random.Number(100, 999).ToString()],
                    },
                PaymentMethod.Check
                    => new CheckPayment()
                    {
                        Student = student,
                        PaymentPeriod = paymentperiod,
                        CheckNumber = faker.Random.Number(1000, 9999).ToString(),
                        BankName = "Bank1",
                        BranchCode = faker.Random.Number(1000, 9999).ToString(),
                    },
                _ => throw new Exception("Invalid payment method"),
            };
            payment.CreatedAt = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;
            payment.PaymentDate = DateTime.Now;
            payment.Amount = faker.Random.Number(100, 1000);
            // set status randomly  from PaymentStatus enum
            payment.Status = (PaymentStatus)
                faker.Random.Number(0, Enum.GetNames(typeof(PaymentStatus)).Length - 1);

            payment.Receipt = new(
                path: null, // i will this nullability in below SeedEntityAsync()
                name: "Receipt of " + payment.Student?.FirstName,
                extension: ".pdf",
                contentType: "application/pdf",
                size: 0,
                description: "Receipt of " + payment.Student?.FirstName
            )
            {
                FileHash = "invalid hash"
            };

            return payment;
        }

        protected override async Task SeedDataAsync()
        {
            _context.Payments ??= _context.Set<Payment>();

            await SeedRandomDataAsync();
            return;

            throw new NotImplementedException("PaymentDBSeeder: SeedDataAsync not implemented yet");
        }

        protected override async Task SeedRandomDataAsync()
        {
            _context.Payments ??= _context.Set<Payment>();

            var dbCount = await _context.Payments.CountAsync();
            if (dbCount >= _maxSeedCount)
            {
                if (_verboseLogging)
                    _logger.LogInformation(
                        $"PaymentDBSeeder: SeedRandomDataAsync already has {dbCount} payments in db"
                    );
                return;
            }

            var payments = GetSeedData();
            try
            {
                foreach (var payment in payments)
                {
                    await SeedEntityAsync(payment);
                    await _context.SaveChangesAsync();
                    if (_seedCount + dbCount >= _maxSeedCount)
                    {
                        if (_verboseLogging)
                            _logger.LogInformation(
                                $"PaymentDBSeeder: SeedRandomDataAsync reached max seed count of {_maxSeedCount}"
                            );
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (_verboseLogging)
                    _logger.LogInformation(
                        $"PaymentDBSeeder: SeedRandomDataAsync could not save changes \n --- \n exception: {e} \n --- \n exception msg: {e.Message}"
                    );
                if (e.InnerException != null && e.InnerException.Message != null)
                    if (_verboseLogging)
                        _logger.LogInformation(
                            $"PaymentDBSeeder: SeedRandomDataAsync could not save change \n --- \n inner exception: {e.InnerException} \n------\n inner exception msg: {e.InnerException.Message}"
                        );
            }
        }

        protected override async Task AfterSeedDataAsync()
        {
            if (_verboseLogging && _seedData.Count > 0)
            {
                _logger.LogInformation("PaymentDBSeeder: AfterSeedDataAsync");
                _logger.LogInformation("We have seed data:");
                foreach (var payment in _seedData)
                {
                    _logger.LogInformation(
                        $"PaymentDBSeeder: AfterSeedDataAsync {payment.Student?.StudentId} {payment.Amount} {payment.PaymentMethod}"
                    );
                }
            }

            // get all payments created in last 10 minutes


            _context.Payments ??= _context.Set<Payment>();

            var tenminutesago = DateTime.Now - TimeSpan.FromMinutes(10);
            // need to take each payment type separately

#pragma warning disable CS8604

            ICollection<CashPayment> dbCashPayments = await _context
                .CashPayments.Where(p => p.CreatedAt > tenminutesago)
                .ToListAsync();

            ICollection<BankPayment> dbBankPayments = await _context
                .BankPayments.Where(p => p.CreatedAt > tenminutesago)
                .ToListAsync();

            ICollection<DebitCardPayment> dbDebitCardPayments = await _context
                .DebitCardPayments.Where(p => p.CreatedAt > tenminutesago)
                .ToListAsync();

            ICollection<CheckPayment> dbCheckPayments = await _context
                .CheckPayments.Where(p => p.CreatedAt > tenminutesago)
                .ToListAsync();

            ICollection<Payment> dbPayments =
            [
                .. dbCashPayments,
                .. dbBankPayments,
                .. dbDebitCardPayments,
                .. dbCheckPayments
            ];

            if (dbPayments == null)
                throw new Exception("PaymentDBSeeder: AfterSeedDataAsync dbPayments is null");

            if (_verboseLogging && dbPayments.Count > 0)
            {
                _logger.LogInformation(
                    $"PaymentDBSeeder: AfterSeedDataAsync we have {dbPayments.Count} payments in db here they are:"
                );

                foreach (var payment in dbPayments)
                {
                    if (_verboseLogging)
                        _logger.LogInformation(
                            $"PaymentDBSeeder: AfterSeedDataAsync {payment.Student?.StudentId} {payment.Amount} {payment.PaymentMethod}"
                        );
                }
            }
        }

        public override async Task SeedEntityAsync(Payment entity)
        {
            // do some validation here befor adding to db
            if (entity == null)
                throw new Exception("PaymentDBSeeder: SeedEntityAsync entity is null");

            if (entity.Student == null)
                throw new Exception("PaymentDBSeeder: SeedEntityAsync entity.Student is null");

            if (entity.PaymentPeriod == null)
                throw new Exception(
                    "PaymentDBSeeder: SeedEntityAsync entity.paymentperiod is null"
                );

            if (entity.Receipt == null)
                throw new Exception("PaymentDBSeeder: SeedEntityAsync entity.Receipt is null");

            if (entity.PaymentPeriod.WorkYear != null)
            {
                entity.PaymentPeriod.WorkYear.School = entity.Student.School;
                entity.PaymentPeriod.WorkYear.PaymentPeriods = new HashSet<PaymentPeriod>();

                entity.PaymentPeriod.WorkYear.CreatedAt = DateTime.Now;
                entity.PaymentPeriod.WorkYear.UpdatedAt = DateTime.Now;
            }

            entity.PaymentPeriod.PerPaymentAmount = entity.Amount;
            entity.PaymentPeriod.Occurrence = Occurrence.Monthly;
            entity.PaymentPeriod.CreatedAt = DateTime.Now;
            entity.PaymentPeriod.UpdatedAt = DateTime.Now;
            if (entity.Receipt.Path == null)
            {
                _context.Receipts ??= _context.Set<Receipt>();
                while (true)
                {
                    var path = faker.Random.Number(1000, 9999).ToString() + ".pdf";
                    if (
                        await _context.Receipts.Where(fp => fp.Path == path).FirstOrDefaultAsync()
                        == null
                    )
                    {
                        entity.Receipt.Path = path;
                        break;
                    }
                }
            }
            entity.Receipt.CreatedBy = entity.Student;
            entity.School = entity.Student.School;

            School? possible_school = _context
                .Schools.Where(s => entity.School != null && entity.School.Name == s.Name)
                .FirstOrDefault();
            if (possible_school != null)
            {
                entity.Student.StudentId = _studentService.GenerateStudentId(possible_school);
            }

            if (_verboseLogging)
                _logger.LogInformation(
                    $"PaymentDBSeeder: SeedEntityAsync {entity.Student?.StudentId} {entity.Amount} {entity.PaymentMethod}"
                );

            try
            {
                _context.Payments.Add(entity);
            }
            catch (Exception e)
            {
                // if exeption is about unique key ignore it but log it

                if (e.InnerException != null && e.InnerException.Message != null)
                {
                    if (e.InnerException.Message.Contains("UNIQUE"))
                    {
                        if (_verboseLogging)
                            _logger.LogInformation(
                                $"PaymentDBSeeder: SeedEntityAsync could not add Payment because of unique key \n exception: {e} \n\n----\n exception msg: {e.Message}"
                            );
                    }
                    if (e.InnerException.Message.Contains("FOREIGN"))
                    {
                        var json = entity.ToJson();
                        if (_verboseLogging)
                            _logger.LogInformation(
                                $"PaymentDBSeeder: SeedEntityAsync could not add Payment, exception: {e} \n\n----\n exception msg: {e.Message} \nn entity: {json}"
                            );
                        _logger.LogError(
                            $"PaymentDBSeeder: SeedEntityAsync could not seed Payment, exception: {e} \n\n----\n exception msg: {e.Message} \n\n entity: {json}"
                        );
                    }
                    if (e.InnerException.Message.Contains("NULL"))
                    {
                        var json = JsonSerializer.Serialize(entity);
                        if (_verboseLogging)
                            _logger.LogInformation(
                                $"PaymentDBSeeder: SeedEntityAsync could not add Payment, exception: {e} \n\n----\n exception msg: {e.Message} \nn entity: {json}"
                            );
                        _logger.LogError(
                            $"PaymentDBSeeder: SeedEntityAsync could not seed Payment, exception: {e} \n\n----\n exception msg: {e.Message} \n\n entity: {json}"
                        );
                    }
                }

                throw;
            }
            _seedCount++;
        }

        private string genIban()
        {
            var iban = "TR";
            for (int i = 0; i < 24; i++)
            {
                iban += faker.Random.Number(0, 9);
            }
            return iban;
        }

        private readonly List<Payment> _seedData = [];
    }
}
