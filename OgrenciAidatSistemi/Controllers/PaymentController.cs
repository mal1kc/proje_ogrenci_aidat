using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Helpers.Controller;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.ViewModels;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class PaymentController(
        ILogger<PaymentController> logger,
        AppDbContext dbContext,
        UserService userService,
        PaymentService paymentService,
        FileService fileService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<PaymentController> _logger = logger;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly UserService _userService = userService;

        private readonly PaymentService _paymentService = paymentService;

        private readonly FileService _fileService = fileService;

        [Authorize]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            searchField ??= "";
            searchString ??= "";
            sortOrder ??= "";

            if (searchField.Length > 70 || searchString.Length > 70 || sortOrder.Length > 70)
            {
                return BadRequest("Search field and search string must be less than 70 characters");
            }
            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;

            ViewBag.UserRole = role;
            if (role == null || role == UserRole.None)
            {
                return RedirectToAction("SignIn", "Home");
            }
            ViewBag.UserRole = role;
            IQueryable<Payment> payments = _dbContext
                .Payments.Include(p => p.Student)
                .Include(p => p.School);
            switch (role)
            {
                case UserRole.SchoolAdmin:
                    if (schoolId != null)
                    {
                        payments = payments
                            .Where(p =>
                                (p.School != null && p.School.Id == schoolId.Value)
                                || (
                                    p.Student != null
                                    && p.Student.School != null
                                    && p.Student.School.Id == schoolId.Value
                                )
                            )
                            .AsQueryable();
                    }
                    else
                    {
                        TempData["Error"] = "School Id is not valid of the current user";
                        return RedirectToAction("SignIn", "Home");
                    }
                    break;
                case UserRole.Student:
                    payments = payments
                        .Where(p =>
                            p.Student != null && p.Student.Id == _userService.GetCurrentUserID()
                        )
                        .AsQueryable();
                    break;
                case UserRole.SiteAdmin:
                    break;
                default:
                    return RedirectToAction("SignIn", "Home");
            }

            var modelList = new QueryableModelHelper<Payment>(payments, Payment.SearchConfig);

            return TryListOrFail(
                () =>
                    modelList.List(
                        ViewData,
                        searchString.SanitizeString(),
                        searchField.SanitizeString(),
                        sortOrder.SanitizeString(),
                        pageIndex,
                        pageSize
                    ),
                "payments"
            );
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Payments = _dbContext.Payments;
            return View();
        }

        // GET: Payment/Delete/5
        [DebugOnly]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.Payments == null)
            {
                _logger.LogError("Payments table is null");
                return NotFound();
            }

            var payment = _dbContext.Payments.FirstOrDefault(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return View(payment.ToView());
        }

        // POST: Payment/Delete/5
        [DebugOnly]
        [
            HttpPost,
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            ValidateAntiForgeryToken
        ]
        public IActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.Payments == null)
            {
                _logger.LogError("Payments table is null");
                return NotFound();
            }

            var payment = _dbContext.Payments.FirstOrDefault(p => p.Id == id);

            if (payment == null)
                return NotFound();

            _dbContext.Payments.Remove(payment);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(List));
        }

        // GET: Payment/Detais/5

        [Authorize]
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();
            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;
            IQueryable<Payment> payments = _dbContext.Payments;
            Payment? payment = null;
            ViewBag.UserRole = role;
            switch (role)
            {
                case UserRole.SiteAdmin:
                    payment = payments.Include(p => p.Student).FirstOrDefault(p => p.Id == id);
                    break;
                case UserRole.SchoolAdmin:
                    if (schoolId.HasValue)
                    {
                        payment = payments
                            .Include(p => p.Student)
                            .FirstOrDefault(p =>
                                (p.Id == id && p.School != null && p.School.Id == schoolId)
                                || (
                                    p.Student != null
                                    && p.Student.School != null
                                    && p.Student.School.Id == schoolId
                                )
                            );
                    }
                    break;
                case UserRole.Student:
                    payment = payments
                        .Include(p => p.Student)
                        .FirstOrDefault(p =>
                            p.Id == id
                            && p.Student != null
                            && p.Student.Id == _userService.GetCurrentUserID()
                        );
                    break;
                default:
                    return Unauthorized();
            }
            if (payment == null)
            {
                TempData["Error"] = "Payment not found";
                return RedirectToAction("List");
            }

            return View(payment.ToView());
        }

        // GET: Payment/PeriodeList
        [Authorize]
        public async Task<IActionResult> PeriodList(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            var (role, schoolId) = await _userService.GetUserRoleAndSchoolId();
            if (role == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserRole = role;

            IQueryable<PaymentPeriod>? paymentPeriods = null;
            switch (role)
            {
                case UserRole.SiteAdmin:
                    paymentPeriods = _dbContext
                        .PaymentPeriods.Include(pp => pp.Student)
                        .Where(p => p.Student != null && p.Student.School != null);
                    break;
                case UserRole.SchoolAdmin:
                    if (schoolId.HasValue)
                    {
                        paymentPeriods = _dbContext
                            .PaymentPeriods.Include(pp => pp.Student)
                            .Where(p =>
                                p.Student != null
                                && p.Student.School != null
                                && p.Student.School.Id == schoolId.Value
                            );
                    }
                    break;
                case UserRole.Student:
                    paymentPeriods = _dbContext.PaymentPeriods.Where(p =>
                        p.Student != null && p.Student.Id == _userService.GetCurrentUserID()
                    );
                    break;
                default:
                    throw new InvalidOperationException("Invalid user role");
            }

            if (paymentPeriods != null)
            {
                var modelList = new QueryableModelHelper<PaymentPeriod>(
                    paymentPeriods,
                    PaymentPeriod.SearchConfig
                );
                searchField ??= "";
                searchString ??= "";
                sortOrder ??= "";

                return TryListOrFail(
                    () =>
                        modelList.List(
                            ViewData,
                            searchString.SanitizeString(),
                            searchField.SanitizeString(),
                            sortOrder.SanitizeString(),
                            pageIndex,
                            pageSize
                        ),
                    "payment periods"
                );
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult PeriodDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;

            var paymentPeriods = _dbContext
                .PaymentPeriods.Include(pp => pp.Student)
                .Include(pp => pp.Payments)
                .Include(pp => pp.WorkYear);
            PaymentPeriod? paymentPeriod = null;
            switch (role)
            {
                case UserRole.SiteAdmin:
                    paymentPeriod = paymentPeriods.FirstOrDefault(p => p.Id == id);
                    break;
                case UserRole.SchoolAdmin:
                    if (schoolId.HasValue)
                    {
                        paymentPeriod = paymentPeriods
                            .Where(p =>
                                p.Id == id
                                && p.Student != null
                                && p.Student.School != null
                                && p.Student.School.Id == schoolId.Value
                            )
                            .FirstOrDefault();
                    }
                    break;
                case UserRole.Student:
                    paymentPeriod = paymentPeriods
                        .Where(p =>
                            p.Id == id
                            && p.Student != null
                            && p.Student.Id == _userService.GetCurrentUserID()
                        )
                        .FirstOrDefault();
                    break;
                default:
                    return Unauthorized();
            }

            if (paymentPeriod == null)
                return NotFound();

            return View(paymentPeriod.ToView());
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult PeriodDelete(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.PaymentPeriods == null)
            {
                _logger.LogError("PaymentPeriods table is null");
                return NotFound();
            }
            return RedirectToAction(nameof(PeriodDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public IActionResult PeriodDeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.PaymentPeriods == null)
            {
                _logger.LogError("PaymentPeriods table is null");
                return NotFound();
            }

            var paymentPeriod = _dbContext.PaymentPeriods.FirstOrDefault(p => p.Id == id);

            if (paymentPeriod == null)
                return NotFound();

            _dbContext.PaymentPeriods.Remove(paymentPeriod);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(PeriodList));
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> PeriodCreate()
        {
            var (role, schoolId) = await _userService.GetUserRoleAndSchoolId();

            if ((schoolId == 0 || schoolId == -1 || schoolId == null) && role != UserRole.SiteAdmin)
            {
                TempData["Error"] = "School is not valid";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.IsSiteAdmin = role == UserRole.SiteAdmin;

            IQueryable<Student>? students =
                role == UserRole.SiteAdmin
                    ? _dbContext.Students
                    : _dbContext.Students.Where(s => s.School != null && s.School.Id == schoolId);

            IQueryable<WorkYear>? workYears =
                role == UserRole.SiteAdmin
                    ? _dbContext.WorkYears
                    : _dbContext.WorkYears.Where(wy =>
                        wy.School != null && wy.School.Id == schoolId
                    );

            ViewBag.Students = students;
            ViewBag.WorkYears = workYears;

            return View();
        }

        [HttpPost]
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PeriodCreate(PaymentPeriodView periodView)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(PeriodList) + "?sortOrder=Id");
            }

            var (role, schoolId) = await _userService.GetUserRoleAndSchoolId();

            if (role == null)
            {
                TempData["Error"] = "User role is not defined";
                return RedirectToAction("PeriodCreate");
            }

            var student = _dbContext
                .Students?.Include(s => s.School)
                .FirstOrDefault(s => s.Id == periodView.StudentId);
            var workYear = _dbContext
                .WorkYears?.Include(wy => wy.School)
                .FirstOrDefault(wy => wy.Id == periodView.WorkYearId);

            if (student == null || workYear == null)
            {
                TempData["Error"] = student == null ? "Student is null" : "WorkYear is null";
                return RedirectToAction("PeriodCreate");
            }

            if (schoolId != null && student.School?.Id != schoolId)
            {
                TempData["Error"] = "User does not have access to this student";
                return RedirectToAction("PeriodCreate");
            }
            bool isValid = periodView.Occurrence switch
            {
                Occurrence.Monthly => periodView.StartDate.AddMonths(1) >= periodView.EndDate,
                Occurrence.Quarterly => periodView.StartDate.AddMonths(3) >= periodView.EndDate,
                Occurrence.Yearly => periodView.StartDate.AddYears(1) >= periodView.EndDate,
                Occurrence.OneTime => periodView.StartDate == periodView.EndDate,
                Occurrence.Daily => periodView.StartDate == periodView.EndDate,
                Occurrence.Weekly => periodView.StartDate.AddDays(7) >= periodView.EndDate,
                _ => false
            };

            if (!isValid)
            {
                TempData["Error"] = "Invalid Occurrence";
                return RedirectToAction("PeriodCreate");
            }

            if (student.School != workYear.School)
            {
                TempData["Error"] = "Student and WorkYear's School is not same";
                return RedirectToAction("PeriodCreate");
            }

            var model = new PaymentPeriod
            {
                Student = student,
                WorkYear = workYear,
                StartDate = periodView.StartDate,
                EndDate = periodView.EndDate,
                PerPaymentAmount = periodView.PerPaymentAmount,
            };

            _dbContext.PaymentPeriods?.Add(model);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(PeriodList));
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public IActionResult PeriodEdit(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.PaymentPeriods == null)
            {
                _logger.LogError("PaymentPeriods table is null");
                return NotFound();
            }

            var paymentPeriod = _dbContext.PaymentPeriods.FirstOrDefault(p => p.Id == id);

            if (paymentPeriod == null)
                return NotFound();

            return View(paymentPeriod.ToView());
        }

        [HttpPost]
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        [ValidateAntiForgeryToken]
        public IActionResult PeriodEdit(int id, PaymentPeriodView periodView)
        {
            if (id != periodView.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var paymentPeriod = _dbContext.PaymentPeriods.FirstOrDefault(p => p.Id == id);

                if (paymentPeriod == null)
                    return NotFound();

                paymentPeriod.StartDate = periodView.StartDate;
                paymentPeriod.EndDate = periodView.EndDate;
                paymentPeriod.PerPaymentAmount = periodView.PerPaymentAmount;

                _dbContext.SaveChanges();
                return RedirectToAction(nameof(PeriodList));
            }
            return View(periodView);
        }

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public IActionResult MakePayment(int? id)
        {
            if (id == null)
                return NotFound();
            var unpaid = _dbContext.Payments.FirstOrDefault(p =>
                p.Id == id && p.PaymentMethod == PaymentMethod.UnPaid
            );

            if (unpaid == null || unpaid.PaymentMethod != PaymentMethod.UnPaid)
            {
                TempData["Error"] = "Payment not found or already paid";
                return NotFound();
            }
            try
            {
                var paymentCreateView = PaymentCreateView.FromUnPaidPayment((UnPaidPayment)unpaid);
                // generate list from enum
                ViewBag.PaymentMethods = new[]
                {
                    PaymentMethod.Bank,
                    PaymentMethod.Check,
                    PaymentMethod.Cash,
                    PaymentMethod.CreditCard
                };

                return View(paymentCreateView);
            }
            catch (Exception e)
            {
                TempData["Error"] = "Error while access payment data";
                _logger.LogError(e, "Error while creating create view from unpaid payment");
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(int id, PaymentCreateView paymentView)
        {
            if (id != paymentView.Id)
                return NotFound();

            var (role, usrId) = await _userService.GetUserRoleAndId();

            var usr = _dbContext.Students.FirstOrDefault(s => s.Id == usrId);
            if (usr == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("SignIn");
            }

            if (ModelState.IsValid)
            {
                UnPaidPayment? payment = (UnPaidPayment?)
                    _dbContext
                        .Payments.Include(p => p.Receipt)
                        .Include(p => p.Student)
                        .Include(p => p.School)
                        .Include(p => p.PaymentPeriod)
                        .FirstOrDefault(p => p.Id == id && p.PaymentMethod == PaymentMethod.UnPaid);
                if (payment == null)
                {
                    TempData["Error"] = "Payment not found or already paid";
                    return NotFound();
                }

                try
                {
                    var new_payment = paymentView.ToAppropriatePayment();

                    if (
                        PaymentCreateView
                            .RequiredFields[paymentView.PaymentMethod]
                            .Contains("Receipt")
                    )
                    {
                        FilePath? receipt_file = null;
                        try
                        {
                            receipt_file = await _fileService.UploadFileAsync(
                                paymentView.Receipt,
                                usr
                            );
                        }
                        catch (Exception e)
                        {
                            TempData["Error"] = "Error while uploading receipt";
                            _logger.LogError(e, "Error while uploading receipt");
                            return RedirectToAction("MakePayment", new { id });
                        }
                        Receipt receipt = Receipt.FromFilePath(receipt_file);
                        new_payment.Receipt = receipt;
                        receipt.Payment = new_payment;
                    }

                    bool result = await _paymentService.MakePayment(payment, new_payment);
                    if (!result)
                    {
                        TempData["Error"] = "Error while making payment";
                        return RedirectToAction("MakePayment", new { id });
                    }
                    else
                    {
                        TempData["Message"] = "Payment made successfully";
                        return RedirectToAction("List");
                    }
                }
                catch (ValidationException e)
                {
                    TempData["Error"] = "Error while making payment";
                    _logger.LogError(e, "Error while making payment");
                    return RedirectToAction("MakePayment", new { id });
                }

                return RedirectToAction("List");
            }
            return RedirectToAction("MakePayment", new { id });
        }

        // verfication of payments
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public IActionResult VerifyPayments()
        {
            var payments = _dbContext
                .Payments.Include(p => p.Student)
                .Include(p => p.School)
                .Where(p => p.Status == PaymentStatus.Paid);

            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;
            if (role == UserRole.SchoolAdmin)
            {
                payments = payments.Where(p => p.School != null && p.School.Id == schoolId);
            }
            var paymentList = payments.ToList();

            return View(paymentList.Select(p => p.ToView()));
        }

        // post method to verify payment
        [HttpPost]
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyPayments(int id)
        {
            var payments = _dbContext.Payments.Where(p =>
                p.Id == id && p.Status == PaymentStatus.Paid
            );
            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;
            Payment? payment = null;
            if (role == UserRole.SiteAdmin)
            {
                payment = payments.FirstOrDefault();
            }
            else if (role == UserRole.SchoolAdmin)
            {
                payment = payments.FirstOrDefault(p => p.School != null && p.School.Id == schoolId);
            }

            if (payment == null)
            {
                TempData["Error"] = "Payment not found";
                return RedirectToAction("VerifyPayments");
            }
            try
            {
                payment.Status = PaymentStatus.Verified;
                _dbContext.Payments.Update(payment);
                _dbContext.SaveChanges();

                ViewData["Success"] =
                    "Payment verified successfully : "
                    + payment.Id
                    + " "
                    + payment.Student?.StudentId;
                return RedirectToAction("VerifyPayments");
            }
            catch (Exception e)
            {
                TempData["Error"] = "Error while verifying payment";
                _logger.LogError(e, "Error while verifying payment");
                return RedirectToAction("VerifyPayments");
            }
        }
    }
}
