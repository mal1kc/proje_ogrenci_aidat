using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.ViewModels;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class SchoolAdminController(
        ILogger<SchoolAdminController> logger,
        AppDbContext dbContext,
        UserService userService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<SchoolAdminController> _logger = logger;

        private readonly AppDbContext _dbContext = dbContext;

        private readonly UserService _userService = userService;

        [Authorize(Roles = Configurations.Constants.userRoles.SchoolAdmin)]
        public async Task<IActionResult> Index()
        {
            var signed_user = await _userService.GetCurrentUserAsync();
            if (signed_user == null)
                return RedirectToAction("SignIn");

            var schoolAdmin = await _dbContext
                .SchoolAdmins.Include(sa => sa.School)
                .Include(sa => sa.ContactInfo)
                .Where(sa => sa.Id == signed_user.Id)
                .FirstOrDefaultAsync();
            if (schoolAdmin == null)
                return RedirectToAction("SignIn");

            var lastPayments = await _dbContext
                .Payments.Include(p => p.Student)
                .Include(p => p.PaymentPeriod)
                .OrderByDescending(p => p.CreatedAt)
                .Where(p =>
                    p.Student != null
                    && p.Student.School != null
                    && p.Student.School.Id == schoolAdmin.School.Id
                    && p.PaymentMethod != PaymentMethod.UnPaid
                )
                .Take(5)
                .ToListAsync();

            var lastStudents = await _dbContext
                .Students.OrderByDescending(s => s.CreatedAt)
                .Where(s => s.School != null && s.School.Id == schoolAdmin.School.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.LastPayments = lastPayments.Select(p => p.ToView()).ToList();

            ViewBag.LastStudents = lastStudents.Select(s => s.ToView()).ToList();

            SchoolAdminView schoolAdminView = schoolAdmin.ToView();
            if (schoolAdminView.School != null)
                schoolAdminView.School.WorkYears = _dbContext
                    .WorkYears.OrderByDescending(wy => wy.StartDate)
                    .Where(wy => wy.School != null && wy.School.Id == schoolAdmin.School.Id)
                    .Take(5)
                    .ToList()
                    .Select(wy => wy.ToView())
                    .ToList();
            ViewBag.UserRole = UserRole.SchoolAdmin;
            return View(schoolAdminView);
        }

        public async Task<IActionResult> SignIn()
        {
            if (await _userService.IsUserSignedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(
            [Bind("EmailAddress", "Password")] SchoolAdminView scAdminView
        )
        {
            scAdminView.PasswordVerify = scAdminView.Password;
            if (!ValidateSignIn(scAdminView, out var validationResult))
            {
                TempData["CantSignIn"] = true;
                return View(scAdminView);
            }

            var passwordHash = _userService.HashPassword(scAdminView.Password);
            var schAdmin = _dbContext.SchoolAdmins.FirstOrDefault(u =>
                u.EmailAddress == scAdminView.EmailAddress && u.PasswordHash == passwordHash
            );

            if (schAdmin == null)
            {
                ModelState.AddModelError("EmailAddress", "User not found");
                return View(scAdminView);
            }

            _logger.LogDebug("User {0} signed in", schAdmin.EmailAddress);

            try
            {
                if (!await _userService.SignInUser(schAdmin, UserRole.SchoolAdmin))
                {
                    TempData["CantSignIn"] = true;
                    TempData["Error"] = "Error while signing in";
                }
                else
                {
                    TempData["Message"] = "Signed in successfully";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while signing in user {0}: {1}",
                    schAdmin.EmailAddress,
                    ex.Message
                );
                TempData["CantSignIn"] = true;
            }

            return RedirectToAction("Index");
        }

        private bool ValidateSignIn(
            SchoolAdminView scAdminView,
            out UserViewValidationResult validationResult
        )
        {
            validationResult = scAdminView.ValidateFieldsSignIn();
            if (validationResult == UserViewValidationResult.FieldsAreValid)
            {
                return true;
            }

            switch (validationResult)
            {
                case UserViewValidationResult.EmailAddressNotMatchRegex:
                    ModelState.AddModelError("EmailAddress", "invalid email syntax");
                    break;
                case UserViewValidationResult.PasswordEmpty:
                    ModelState.AddModelError("Password", "Password is empty");
                    break;
            }

            return false;
        }

        /*
        ├ ƒ List(string searchString = null, string searchField = null, string sortOrder = null, int pageIndex = 1, int pageSize = 20)
        ├ ƒ Create()
        ├ ƒ Create(SiteAdminView siteAdmin)
        ├ ƒ Delete(int? id)
        └ ƒ DeleteConfirmed(int? id)
        */

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> List(
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
            var (usrRole, schId) = await _userService.GetUserRoleAndSchoolId();
            if (usrRole == UserRole.Student)
                return RedirectToAction("Index", "Home");
            IQueryable<SchoolAdmin> schoolAdmins = _dbContext.SchoolAdmins.Include(sa => sa.School);
            if (usrRole == UserRole.SchoolAdmin)
            {
                schoolAdmins = schoolAdmins.Where(sa => sa.School.Id == schId);
            }
            var modelList = new QueryableModelHelper<SchoolAdmin>(
                schoolAdmins,
                SchoolAdmin.SearchConfig
            );

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
                "school admins"
            );
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Schools = _dbContext.Schools;
            var scAdminView = new SchoolAdminView { ContactInfo = new ContactInfoView() };
            return View(scAdminView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Create(SchoolAdminView scAdminView)
        {
            ViewBag.Schools = _dbContext.Schools;
            UserViewValidationResult validationResult = scAdminView.ValidateFieldsCreate(
                _dbContext
            );
            // check is school exists in db
            if (validationResult != UserViewValidationResult.FieldsAreValid)
            {
                TempData["CantCreate"] = true;
                switch (validationResult)
                {
                    case UserViewValidationResult.EmailAddressNotMatchRegex:
                        ModelState.AddModelError("EmailAddress", "invalid email syntax");
                        break;
                    case UserViewValidationResult.PasswordsNotMatch:
                        ModelState.AddModelError("Password", "Passwords do not match");
                        break;
                    case UserViewValidationResult.PasswordEmpty:
                        ModelState.AddModelError("Password", "Password is empty");
                        break;
                    case UserViewValidationResult.UserExists:
                        ModelState.AddModelError("EmailAddress", "User already exists");
                        break;
                    case UserViewValidationResult.InvalidName:
                        ModelState.AddModelError("FirstName", "Invalid first name or last name");
                        break;
                    case UserViewValidationResult.EmailAddressExists:
                        ModelState.AddModelError("EmailAddress", "Email already exists");
                        break;
                }
                return View(scAdminView);
            }
            var school = _dbContext
                .Schools?.Where(s => s.Id == scAdminView.SchoolId)
                .FirstOrDefault();

            if (school == null)
            {
                TempData["CantCreate"] = true;
                ModelState.AddModelError("School", "School is required");
                return View(scAdminView);
            }

            // check if user exists
            _logger.LogDebug("Creating user {0}", scAdminView.EmailAddress);
            try
            {
                var newSchAdmin = new SchoolAdmin
                {
                    EmailAddress = scAdminView.EmailAddress,
                    FirstName = scAdminView.FirstName,
                    LastName = scAdminView.LastName,
                    PasswordHash = _userService.HashPassword(scAdminView.Password),
                    School = school,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                newSchAdmin.ContactInfo = new ContactInfo
                {
                    Email = newSchAdmin.EmailAddress,
                    PhoneNumber = scAdminView.ContactInfo.PhoneNumber,
                    Addresses = scAdminView.ContactInfo.Addresses
                };
                _logger.LogDebug("User {0} created", scAdminView.EmailAddress);

                _dbContext.SchoolAdmins.Add(newSchAdmin);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while creating user {0}: {1}",
                    scAdminView.EmailAddress,
                    ex.Message
                );
                if (ex.InnerException != null)
                    _logger.LogError("inner exception: {0}", ex.InnerException.Message);
                if (ex.InnerException?.InnerException != null)
                    _logger.LogError(
                        "inner inner exception: {0}",
                        ex.InnerException.InnerException.Message
                    );
                TempData["CantCreate"] = true;
                return RedirectToAction("Create");
            }
            // Succes state remove schools from viewbag
            ViewBag.Schools = null;
            return RedirectToAction("List");
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _dbContext.SchoolAdmins == null)
                return NotFound();
            var siteAdmin = await _dbContext
                .SchoolAdmins.Include(sa => sa.School)
                .Where(sa => sa.Id == id)
                .FirstOrDefaultAsync();
            if (siteAdmin == null)
                return NotFound();

            // check if the user is logged in
            // if the user is logged in, prevent deletion

            var loggedInUserId = _userService.GetSignedInUserId();
            _logger.LogDebug("Logged in user id: {}", loggedInUserId);

            if (loggedInUserId == id)
            {
                ViewData["Message"] = "You cannot delete the account you are logged in with";
                return RedirectToAction("List");
            }

            return View(siteAdmin.ToView());
        }

        [
            HttpPost,
            ActionName("DeleteConfirmed"),
            Authorize(
                Roles = Configurations.Constants.userRoles.SiteAdmin
                    + ","
                    + Configurations.Constants.userRoles.SchoolAdmin
            ),
            ValidateAntiForgeryToken
        ]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();
            bool isUserDeleted = await _userService.DeleteUser((int)id);
            if (!isUserDeleted)
                ViewData["Message"] = "Error while deleting user";
            return RedirectToAction("List");
        }

        // GET: SchoolAdmin/Detalis/5
        //  only accessible by site admin
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _dbContext.SchoolAdmins == null)
                return NotFound();
            var siteAdmin = await _dbContext
                .SchoolAdmins.Include(sa => sa.School)
                .Include(sa => sa.ContactInfo)
                .Where(sa => sa.Id == id)
                .FirstOrDefaultAsync();
            if (siteAdmin == null)
                return NotFound();
            return View(siteAdmin.ToView());
        }
    }
}
