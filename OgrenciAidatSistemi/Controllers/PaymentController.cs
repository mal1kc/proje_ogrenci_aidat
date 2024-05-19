using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Helpers.Controller;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    // TODO: change authorization roles for actions
    // debug : for only edit,delete action
    // must be SiteAdmin or SchoolAdmin (for its school) => list, create, details
    // or Student (for its own Payments) => details, list , create

    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly AppDbContext _dbContext;
        private readonly UserService _userService;

        public PaymentController(
            ILogger<PaymentController> logger,
            AppDbContext dbContext,
            UserService userService
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _userService = userService;
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
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
            if (_dbContext.Payments == null)
            {
                _logger.LogError("Payments table is null");
                _dbContext.Payments = _dbContext.Set<Payment>();
            }
            var modelList = new QueryableModelHelper<Payment>(
                _dbContext.Payments.AsQueryable(),
                Payment.SearchConfig
            );
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
                _dbContext.Payments = _dbContext.Set<Payment>();
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
                _dbContext.Payments = _dbContext.Set<Payment>();
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

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.Payments == null)
            {
                _logger.LogError("Payments table is null");
                _dbContext.Payments = _dbContext.Set<Payment>();
                return NotFound();
            }

            var payment = _dbContext
                .Payments.Include(p => p.Student)
                .FirstOrDefault(p => p.Id == id);

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
            // get current user if not admin (site or school) means student , get own paymentperiods else
            // siteadmin => all periodes
            // schooladmin => school's student's periodes

            var user = await _userService.GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            IQueryable<PaymentPeriod>? paymentPeriods = null;

            switch (user.Role)
            {
                case UserRole.SiteAdmin:
                    paymentPeriods = _dbContext.PaymentPeriods.Include(pp => pp.Student);
                    break;
                case UserRole.SchoolAdmin:
                    if (_dbContext.SchoolAdmins == null)
                    {
                        _logger.LogError("SchoolAdmins table is null");
                        _dbContext.SchoolAdmins = _dbContext.Set<SchoolAdmin>();
                    }
                    SchoolAdmin? schadmin = await _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefaultAsync(u => u.Id == user.Id);
                    if (schadmin != null)
                    {
                        paymentPeriods = _dbContext
                            .PaymentPeriods?.Include(pp => pp.Student)
                            .Where(p => p.Student.School.Id == schadmin.School!.Id);
                    }
                    break;
                case UserRole.Student:
                    if (_dbContext.Students == null)
                    {
                        _logger.LogError("Students table is null");
                        _dbContext.Students = _dbContext.Set<Student>();
                    }
                    var student = await _dbContext.Students.FirstOrDefaultAsync(u =>
                        u.Id == user.Id
                    );
                    if (student == null)
                    {
                        return NotFound();
                    }
                    if (_dbContext.PaymentPeriods == null)
                    {
                        _logger.LogError("PaymentPeriods table is null");
                        _dbContext.PaymentPeriods = _dbContext.Set<PaymentPeriod>();
                    }
                    paymentPeriods = _dbContext.PaymentPeriods.Where(p =>
                        p.Student.Id == student.Id
                    );
                    break;
                default:
                    throw new InvalidOperationException("Invalid user role");
            }
            ;

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
                return NotFound();
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
                _dbContext.PaymentPeriods = _dbContext.Set<PaymentPeriod>();
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

        public IActionResult PaymentListPartial(HashSet<PaymentView> model)
        {
            return PartialView("_PaymentListPartial", model);
        }

        public IActionResult PeriodDelete(int? id)
        {
            if (id == null)
                return NotFound();

            if (_dbContext.PaymentPeriods == null)
            {
                _logger.LogError("PaymentPeriods table is null");
                _dbContext.PaymentPeriods = _dbContext.Set<PaymentPeriod>();
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
                _dbContext.PaymentPeriods = _dbContext.Set<PaymentPeriod>();
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

            #region get user role and id
            string? stringRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            int? usrId = null;
            string nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (stringRole == null || nameIdentifier == null)
            {
                _logger.LogError("User role is null");
                return RedirectToAction("Index", "Home");
            }
            usrId = int.Parse(nameIdentifier);
            UserRole userRole = UserRoleExtensions.GetRoleFromString(stringRole);
            #endregion
            #region check user role and get data owned by user

            IQueryable<School>? schools = null;
            IQueryable<Student>? students = null;
            IQueryable<WorkYear>? workYears = null;

            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    students = _dbContext.Students.AsQueryable();
                    workYears = _dbContext.WorkYears.AsQueryable();
                    ViewBag.IsSiteAdmin = true;
                    break;
                case UserRole.SchoolAdmin:
                    var schadmin = _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefault(sa => sa.Id == usrId);
                    if (schadmin == null)
                    {
                        _logger.LogError("SchoolAdmin is null");
                        return RedirectToAction("Index", "Home");
                    }
                    students = _dbContext.Students.Where(s => s.School.Id == schadmin.School.Id);
                    workYears = _dbContext.WorkYears.Where(wy =>
                        wy.School.Id == schadmin.School.Id
                    );
                    break;
                default:
                    _logger.LogError("User role is not valid");
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
            // TODO: use claim to corretly set determine the user role
            // based on role restrict the access to creation operation
            if (ModelState.IsValid)
            {
                var student = _dbContext.Students.FirstOrDefault(s => s.Id == periodView.StudentId);
                if (student == null)
                {
                    TempData["Error"] = "Student is null";
                    return RedirectToAction("PeriodCreate");
                }
                var workYear = _dbContext.WorkYears.FirstOrDefault(wy =>
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

                _dbContext.PaymentPeriods.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(PeriodList));
            }
            // redirect to list with sort
            return RedirectToAction(nameof(PeriodList) + "?sortOrder=Id");
        }
    }
}
