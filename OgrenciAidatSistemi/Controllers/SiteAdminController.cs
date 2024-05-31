using System.Data;
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
    public class SiteAdminController(
        ILogger<SiteAdminController> logger,
        AppDbContext appDbContext,
        UserService userService,
        ExportService exportService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<SiteAdminController> _logger = logger;
        private readonly AppDbContext _appDbContext = appDbContext;

        private readonly UserService _userService = userService;

        private readonly ExportService _exportService = exportService;

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Index()
        {
            var current_user_id = _userService.GetCurrentUserID();
            if (current_user_id == null)
            {
                return RedirectToAction("SignOut", "SiteAdmin");
            }
            var latestSiteAdmins = _appDbContext
                .SiteAdmins.OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();
            var latestStudents = _appDbContext
                .Students.Include(x => x.School)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();
            var latestPayments = _appDbContext
                .Payments.Include(x => x.School)
                .Include(x => x.Student)
                .Include(x => x.PaymentPeriod)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();
            var latestPaymentsPeriods = _appDbContext
                .PaymentPeriods.Include(x => x.Payments)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();
            var latestReceipts = _appDbContext
                .Receipts.Include(x => x.Payment)
                .Include(x => x.CreatedBy)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.LatestSiteAdmins = latestSiteAdmins.Select(x => x.ToView());
            ViewBag.LatestStudents = latestStudents.Select(x => x.ToView());
            ViewBag.LatestPayments = latestPayments.Select(x => x.ToView());
            ViewBag.LatestPaymentsPeriods = latestPaymentsPeriods.Select(x => x.ToView());
            ViewBag.LatestReceipts = latestReceipts.Select(x => x.ToView());

            var current_site_admin = _appDbContext.SiteAdmins.Find(current_user_id);
            if (current_site_admin == null)
            {
                return RedirectToAction("SignOut", "SiteAdmin");
            }

            return View(current_site_admin.ToView());
        }

        [HttpGet(Configurations.Constants.AdminAuthenticationLoginPath)]
        public async Task<IActionResult> SignIn()
        {
            if (await _userService.IsUserSignedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost(Configurations.Constants.AdminAuthenticationLoginPath)]
        public async Task<IActionResult> SignInPost(SiteAdminView adminView)
        {
            if (adminView.Username == null || adminView.Password == null)
            {
                TempData["CantSignIn"] = true;
                return RedirectToAction("SignIn");
            }

            var givenPasswordHash = SiteAdmin.ComputeHash(adminView.Password);
            _appDbContext.SiteAdmins ??= _appDbContext.Set<SiteAdmin>();
            SiteAdmin? admin = _appDbContext
                .SiteAdmins.Where(_admin =>
                    _admin.Username == adminView.Username
                    && _admin.PasswordHash == givenPasswordHash
                )
                .FirstOrDefault();
            if (admin == null)
            {
                TempData["CantSignIn"] = true;
                _logger.LogDebug("Cannot Find SiteAdmin that matches the given credentials");
                return RedirectToAction("SignIn");
            }

            _logger.LogDebug("SiteAdmin found, signing in {0}", admin.Username);

            try
            {
                if (!await _userService.SignInUserAsync(admin))
                {
                    TempData["CantSignIn"] = true;
                    return RedirectToAction("SignIn");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while signing in SiteAdmin {0}: {1}",
                    admin.Username,
                    ex.Message
                );
                TempData["CantSignIn"] = true;
                return RedirectToAction("SignIn");
            }
            return RedirectToAction("Index", "Home");
        }

        // path: /siteadmin/list
        [HttpGet, Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
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
            _appDbContext.SiteAdmins ??= _appDbContext.Set<SiteAdmin>();

            var modelHelper = new QueryableModelHelper<SiteAdmin>(
                _appDbContext.SiteAdmins.AsQueryable(),
                SiteAdmin.SearchConfig
            );
            searchField ??= "";
            searchString ??= "";
            sortOrder ??= "";

            return TryListOrFail(
                () =>
                    modelHelper.List(
                        ViewData,
                        searchString.SanitizeString(),
                        searchField.SanitizeString(),
                        sortOrder.SanitizeString(),
                        pageIndex,
                        pageSize
                    ),
                "site admins"
            );
        }

        // Create a new SiteAdmin
        // GET: /siteadmin/create

        [HttpGet, Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /siteadmin/create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for

        [
            HttpPost,
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            ValidateAntiForgeryToken
        ]
        public async Task<IActionResult> Create(
            [Bind("Username,Password,PasswordVerify,FirstName,LastName,EmailAddress")]
                SiteAdminView siteAdmin
        )
        {
            if (ModelState.IsValid)
            {
                UserViewValidationResult validationResult = siteAdmin.ValidateFieldsSignUp(
                    _appDbContext
                );
                switch (validationResult)
                {
                    case UserViewValidationResult.PasswordsNotMatch:
                        ModelState.AddModelError("PasswordVerify", "Passwords do not match");
                        break;
                    case UserViewValidationResult.InvalidName:
                        ModelState.AddModelError("FirstName", "Name is too short or too long");
                        break;
                    case UserViewValidationResult.EmailAddressNotMatchRegex:
                        ModelState.AddModelError("EmailAddress", "Invalid Email Address");
                        break;
                    case UserViewValidationResult.UserExists:
                        ModelState.AddModelError("Username", "Username already exists");
                        break;
                    default:
                        break;
                }
                if (validationResult != UserViewValidationResult.FieldsAreValid)
                    return View(siteAdmin);

                SiteAdmin newSiteAdmin =
                    new()
                    {
                        Username = siteAdmin.Username,
                        PasswordHash = SiteAdmin.ComputeHash(siteAdmin.Password),
                        EmailAddress = siteAdmin.EmailAddress,
                        FirstName = siteAdmin.FirstName,
                        LastName = siteAdmin.LastName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                _appDbContext.SiteAdmins ??= _appDbContext.Set<SiteAdmin>();

                try
                {
                    _appDbContext.SiteAdmins.Add(newSiteAdmin);

                    await _appDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while adding new site admin: {0}", ex.Message);
                    return View(siteAdmin);
                }
                return RedirectToAction("List");
            }
            return View(siteAdmin);
        }

        // GET: /SiteAdmin/Delete/5
        // Delete a SiteAdmin
        // This is a confirmation page for deleting a SiteAdmin

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        [DebugOnly]
        // [DisabledAction]
        public Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            _appDbContext.SiteAdmins ??= _appDbContext.Set<SiteAdmin>();
            var siteAdmin = _appDbContext.SiteAdmins.Where(e => e.Id == id).FirstOrDefault();

            if (siteAdmin == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            // check if the user is logged in
            // if the user is logged in, prevent deletion

            var loggedInUserId = _userService.GetSignedInUserId();
            _logger.LogInformation("Logged in user id: {}", loggedInUserId);

            if (loggedInUserId == id)
            {
                ViewData["Message"] = "You cannot delete the account you are logged in with";
                return Task.FromResult<IActionResult>(RedirectToAction("List"));
            }

            return Task.FromResult<IActionResult>(View(siteAdmin.ToView()));
        }

        // POST: /SiteAdmin/DeleteConfirmed/5
        // Delete a SiteAdmin
        //
        //
        // // This is a hard delete, meaning the record will be removed from the database
        // and cannot be recovered
        // by default, this action is disabled
        // to enable this action, remove the [DisabledAction] attribute

        // in enable is not possible to delete logged in SiteAdmin
        // if you want to delete a SiteAdmin, you must first sign in with another SiteAdmin account
        // and then delete the account you want to delete
        // this is a security measure to prevent accidental deletion of all SiteAdmin accounts


        [DebugOnly]
        [
            HttpPost,
            ActionName("DeleteConfirmed"),
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            ValidateAntiForgeryToken
        ]
        // [DisabledAction]

        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null || _appDbContext.Users == null)
                return NotFound();

            var dbUser = _appDbContext.Users.Where(e => e.Id == id).FirstOrDefault();

            if (dbUser == null)
                return NotFound();

            bool isDeleted = await _userService.DeleteUser(dbUser.Id);
            if (!isDeleted)
                ViewData["Message"] = "Error while deleting the user";
            return RedirectToAction("List");
        }

        // GET: /SiteAdmin/Details/5
        // Show details of a SiteAdmin
        // This page is read-only // This page is accessible by siteAdmins only

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _appDbContext.SiteAdmins == null)
                return NotFound();
            var siteAdmin = await _appDbContext.SiteAdmins.FindAsync(id);
            if (siteAdmin == null)
                return NotFound();
            return View(siteAdmin.ToView());
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        [HttpGet]
        public IActionResult Export()
        {
            throw new NotImplementedException();
        }
    }
}
