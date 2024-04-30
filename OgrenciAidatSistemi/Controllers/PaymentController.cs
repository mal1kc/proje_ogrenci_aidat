using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

#warning "This action not tested"
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
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
