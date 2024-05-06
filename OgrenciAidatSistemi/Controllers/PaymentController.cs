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
            // get current user if not admin (site or school) means student , get own paymentperiodes else
            // siteadmin => all periodes
            // schooladmin => school's student's periodes

            var user = await _userService.GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            IQueryable<PaymentPeriode>? paymentPeriods = null;

            switch (user.Role)
            {
                case UserRole.SiteAdmin:
                    paymentPeriods = _dbContext.PaymentPeriods;
                    break;
                case UserRole.SchoolAdmin:
                    var schadmin = await _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefaultAsync(u => u.Id == user.Id);
                    if (schadmin != null)
                    {
                        paymentPeriods = _dbContext
                            .PaymentPeriods?.Include(pp => pp.Student)
                            .Where(p => p.Student.School.Id == schadmin.School.Id);
                    }
                    break;
                case UserRole.Student:
                    var student = await _dbContext.Students.FirstOrDefaultAsync(u =>
                        u.Id == user.Id
                    );
                    paymentPeriods = _dbContext.PaymentPeriods.Where(p =>
                        p.Student.Id == student.Id
                    );
                    break;
                default:
                    throw new InvalidOperationException("Invalid user role");
            }
            ;

            var modelList = new QueryableModelHelper<PaymentPeriode>(
                paymentPeriods,
                PaymentPeriode.SearchConfig
            );
            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
            );
        }

#warning "This action not tested"

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
            // TODO: impelment Paymentinfo delete action
            throw new NotImplementedException("Delete action not implemented");
        }

        // POST: Payment/Delete/5
        [DebugOnly]
        [
            HttpPost,
            ActionName("Delete"),
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            ValidateAntiForgeryToken
        ]
        public IActionResult DeleteConfirmed(int? id)
        {
            // TODO: implement Paymentinfo delete action
            throw new NotImplementedException("DeleteConfirmed action not implemented");
        }

        // GET: Payment/Detais/5

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Details(int? id)
        {
            // TODO: implement Paymentinfo details action
            throw new NotImplementedException("Details action not implemented");
        }
    }
}
