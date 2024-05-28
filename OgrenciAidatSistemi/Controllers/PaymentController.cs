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
    // TODO: change authorization roles for actions
    // debug : for only edit,delete action
    // must be SiteAdmin or SchoolAdmin (for its school) => list, create, details
    // or Student (for its own Payments) => details, list , create

    public class PaymentController(
        ILogger<PaymentController> logger,
        AppDbContext dbContext,
        UserService userService
    ) : Controller
    {
        private readonly ILogger<PaymentController> _logger = logger;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly UserService _userService = userService;

        [Authorize]
        [DebugOnly]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
#if DEBUG
            ViewBag.IsDebug = true;
#endif
            IQueryable<Payment>? payments = null;

            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;

            ViewBag.UserRole = role;
            switch (role)
            {
                case UserRole.SiteAdmin:
                    payments = _dbContext.Payments.Include(p => p.Student).Include(p => p.School);
                    ViewBag.UserRole = UserRole.SiteAdmin;
                    break;
                case UserRole.SchoolAdmin:
                    if (schoolId.HasValue)
                    {
                        payments = _dbContext
                            .Payments.Include(p => p.Student)
                            .Where(p => p.School != null && p.School.Id == schoolId.Value);
                    }
                    ViewBag.UserRole = UserRole.SchoolAdmin;
                    break;
                case UserRole.Student:
                    payments = _dbContext
                        .Payments.Include(p => p.Student)
                        .Where(p =>
                            p.Student != null && p.Student.Id == _userService.GetCurrentUserID()
                        );
                    ViewBag.UserRole = UserRole.Student;
                    break;
                default:
                    return Unauthorized();
            }

            var modelList = new QueryableModelHelper<Payment>(payments, Payment.SearchConfig);

            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
            );
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Payments = _dbContext.Payments;
            return View();
        }

        // POST: Payment/Create

        [HttpPost]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payment model)
        {
            // TODO:: implement Paymentinfo create action
            throw new NotImplementedException("Create action not implemented");
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
            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;
            Payment? payment = null;
            switch (role)
            {
                case UserRole.SiteAdmin:
                    payment = _dbContext
                        .Payments.Include(p => p.Student)
                        .FirstOrDefault(p => p.Id == id);
                    break;
                case UserRole.SchoolAdmin:
                    if (schoolId.HasValue)
                    {
                        payment = _dbContext
                            .Payments.Include(p => p.Student)
                            .FirstOrDefault(p =>
                                p.Id == id && p.School != null && p.School.Id == schoolId.Value
                            );
                    }
                    break;
                case UserRole.Student:
                    payment = _dbContext
                        .Payments.Include(p => p.Student)
                        .FirstOrDefault(p =>
                            p.Id == id
                            && p.Student != null
                            && p.Student.Id == _userService.GetCurrentUserID()
                        );
                    break;
                default:
                    return Unauthorized();
            }
            if (id == null)
                return NotFound();

            if (payment == null)
                return NotFound();

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
                return View(
                    modelList.List(
                        ViewData,
                        searchString,
                        searchField,
                        sortOrder,
                        pageIndex,
                        pageSize
                    )
                );
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult PeriodDetails(int? id)
        {
            // TODO: add correct roles
            if (id == null)
                return NotFound();

            if (_dbContext.PaymentPeriods == null)
            {
                _logger.LogError("PaymentPeriods table is null");
                return NotFound();
            }

            var paymentPeriod = _dbContext
                .PaymentPeriods.Include(pp => pp.Student)
                .Include(pp => pp.Payments)
                .Include(pp => pp.WorkYear)
                .FirstOrDefault(pp => pp.Id == id);

            if (paymentPeriod == null)
                return NotFound();

            return View(paymentPeriod.ToView());
        }

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
        public IActionResult PeriodCreate()
        {
            ViewBag.IsSiteAdmin = false;
            IQueryable<Student>? students = null;
            IQueryable<WorkYear>? workYears = null;

            #region get user role and groupSid claim to determine the user role

            var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;

            ViewBag.IsSiteAdmin = role == UserRole.SiteAdmin;

            switch (role)
            {
                case UserRole.SiteAdmin:
                    students = _dbContext.Students;
                    workYears = _dbContext.WorkYears;
                    break;
                case UserRole.SchoolAdmin:
                    try
                    {
                        if (schoolId == 0 || schoolId == -1 || schoolId == null)
                        {
                            throw new FormatException("schoolId is not valid");
                        }
                        students = _dbContext.Students.Where(s =>
                            s.School != null && s.School.Id == schoolId
                        );
                        workYears = _dbContext.WorkYears.Where(wy =>
                            wy.School != null && wy.School.Id == schoolId
                        );
                        break;
                    }
                    catch (FormatException e)
                    {
                        _logger.LogError(e, e.Message ?? "schoolId is not valid");
                    }

                    TempData["Error"] = "School is not valid";
                    return RedirectToAction("Index", "Home");
                default:
                    TempData["Error"] = "User role is not valid";
                    return RedirectToAction("Index", "Home");
            }
            #endregion

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
        public IActionResult PeriodCreate(PaymentPeriodView periodView)
        {
            if (ModelState.IsValid)
            {
                var (role, schoolId) = _userService.GetUserRoleAndSchoolId().Result;
                if (role == null)
                {
                    TempData["Error"] = "User role is not defined";
                    return RedirectToAction("PeriodCreate");
                }

                var student = _dbContext.Students?.FirstOrDefault(s =>
                    s.Id == periodView.StudentId
                );
                if (student == null)
                {
                    TempData["Error"] = "Student is null";
                    return RedirectToAction("PeriodCreate");
                }

                // If the user's school ID doesn't match the student's school ID, return an error
                if (schoolId != null && student.School?.Id != schoolId)
                {
                    TempData["Error"] = "User does not have access to this student";
                    return RedirectToAction("PeriodCreate");
                }

                var workYear = _dbContext.WorkYears?.FirstOrDefault(wy =>
                    wy.Id == periodView.WorkYearId
                );
                if (workYear == null)
                {
                    _logger.LogError("WorkYear is null");
                    TempData["Error"] = "WorkYear is null";
                    return RedirectToAction("PeriodCreate");
                }

                // check if period is already exists at least 30 days
                if (periodView.StartDate.AddDays(30) > periodView.EndDate)
                {
                    TempData["Error"] = "Period is less than 30 days";
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
                    Payments = new HashSet<Payment>()
                };

                _dbContext.PaymentPeriods?.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(PeriodList));
            }
            // redirect to list with sort
            return RedirectToAction(nameof(PeriodList) + "?sortOrder=Id");
        }
    }
}
