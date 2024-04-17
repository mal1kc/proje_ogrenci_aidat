using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class SchoolAdminController : Controller
    {
        private readonly ILogger<SchoolAdminController> _logger;

        private readonly AppDbContext _dbContext;

        private readonly UserService _userService;

        public SchoolAdminController(
            ILogger<SchoolAdminController> logger,
            AppDbContext dbContext
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _userService = new UserService(dbContext, new HttpContextAccessor());
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SchoolAdmin)]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SchoolAdmin)]
        public IActionResult SignIn()
        {
            if (_userService.IsUserSignedIn())
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
            // some idoitic validation
            scAdminView.PasswordVerify = scAdminView.Password;
            UserViewValidationResult validationResult = scAdminView.ValidateFieldsSignIn();
            if (validationResult != UserViewValidationResult.FieldsAreValid)
            {
                TempData["CantSignIn"] = true;
                switch (validationResult)
                {
                    case UserViewValidationResult.EmailAddressNotMatchRegex:
                        ModelState.AddModelError("EmailAddress", "invalid email syntax");
                        break;
                    case UserViewValidationResult.PasswordEmpty:
                        ModelState.AddModelError("Password", "Password is empty");
                        break;
                }
                return View(scAdminView);
            }

            // check if user exists
            // TODO: maybe need to change email to username
            var passwordHash = _userService.HashPassword(scAdminView.Password);
            var schAdmin = _dbContext
                .SchoolAdmins.Where(u =>
                    u.EmailAddress == scAdminView.EmailAddress && u.PasswordHash == passwordHash
                )
                .FirstOrDefault();
            if (schAdmin == null)
            {
                ModelState.AddModelError("EmailAddress", "User not found");
            }

            _logger.LogDebug("User {0} signed in", schAdmin.Username);

            try
            {
                await _userService.SignInUser(schAdmin, UserRole.SiteAdmin);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while signing in user {0}: {1}",
                    schAdmin.Username,
                    ex.Message
                );
                TempData["CantSignIn"] = true;
                return RedirectToAction("SignIn");
            }
            return RedirectToAction("Index");
        }

        /*
        ├ ƒ List(string searchString = null, string searchField = null, string sortOrder = null, int pageIndex = 1, int pageSize = 20)
        ├ ƒ Create()
        ├ ƒ Create(SiteAdminView siteAdmin)
        ├ ƒ Delete(int? id)
        └ ƒ DeleteConfirmed(int? id)
        */

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult List(
            string searchString = null,
            string searchField = null,
            string sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {



            // TODO improve modelSearchConfig add
            // 1. viewdata keys ; sortorder, searchstring, currentfield
            // TODO improve QueryableModelHelper do sortig and searching in one method with given config
            // ++ improve logics for searching and sorting
            if (_dbContext.SchoolAdmins == null)
            {
                _logger.LogError("SchoolAdmins table is null");
                _dbContext.SchoolAdmins = _dbContext.Set<SchoolAdmin>();
            }

            var modelList = new QueryableModelHelper<SchoolAdmin>(
                    _dbContext.SchoolAdmins.AsQueryable(),
                    new ModelSearchConfig(
                        SchoolAdminSearchConfig.AllowedFieldsForSearch,
                        SchoolAdminSearchConfig.AllowedFieldsForSort
                    )
                    );

            return View(modelList.List(
                ViewData,
                searchString,
                searchField,
                sortOrder,
                pageIndex,
                pageSize
            ));
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Schools = _dbContext.Schools;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Create(
            [Bind("Username", "EmailAddress", "Password", "PasswordVerify",
                "SchoolId", "FirstName", "LastName"
                )] SchoolAdminView scAdminView
        )
        {
            ViewBag.Schools = _dbContext.Schools;
            UserViewValidationResult validationResult = scAdminView.ValidateFieldsCreate(_dbContext);
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
                        ModelState.AddModelError("Username", "User already exists");
                        break;
                    case UserViewValidationResult.InvalidName:
                    case UserViewValidationResult.InvalidUsername:
                        ModelState.AddModelError("Username", "Invalid username");
                        break;
                    case UserViewValidationResult.EmailAddressExists:
                        ModelState.AddModelError("EmailAddress", "Email already exists");
                        break;
                }
                return View(scAdminView);
            }
            var school = _dbContext.Schools?.Where(s => s.Id == scAdminView.SchoolId).FirstOrDefault();

            if (school == null)
            {
                TempData["CantCreate"] = true;
                ModelState.AddModelError("School", "School is required");
                return View(scAdminView);
            }

            // check if user exists
            _logger.LogDebug("Creating user {0}", scAdminView.Username);
            try
            {
                var newSchAdmin = new SchoolAdmin
                {
                    Username = scAdminView.Username,
                    EmailAddress = scAdminView.EmailAddress,
                    FirstName = scAdminView.FirstName,
                    LastName = scAdminView.LastName,
                    PasswordHash = _userService.HashPassword(scAdminView.Password),
                    _School = school,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _logger.LogDebug("User {0} created", scAdminView.Username);
                _logger.LogDebug("saving user {0} to db", scAdminView.Username);
                if (_dbContext.SchoolAdmins == null)
                {
                    _dbContext.SchoolAdmins = _dbContext.Set<SchoolAdmin>();
                }
                _dbContext.SchoolAdmins.Add(newSchAdmin);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while creating user {0}: {1}",
                    scAdminView.Username,
                    ex.Message
                );
                if (ex.InnerException != null)
                    _logger.LogError("inner exception: {0}", ex.InnerException.Message);
                if (ex.InnerException?.InnerException != null)
                    _logger.LogError("inner inner exception: {0}", ex.InnerException.InnerException.Message);
                TempData["CantCreate"] = true;
                return RedirectToAction("Create");
            }
            // Succes state remove schools from viewbag
            ViewBag.Schools = null;
            return RedirectToAction("List");
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siteAdmin = _dbContext.SchoolAdmins.Where(e => e.Id == id).FirstOrDefault();

            if (siteAdmin == null)
            {
                return NotFound();
            }

            // check if the user is logged in
            // if the user is logged in, prevent deletion

            var loggedInUserId = _userService.GetSignedInUserId();
            Console.WriteLine("Logged in user id: {0}", loggedInUserId);

            if (loggedInUserId == id)
            {
                ViewData["Message"] = "You cannot delete the account you are logged in with";
                return RedirectToAction("List");
            }

            return View(siteAdmin);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]

        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            bool isUserDeleted = await _userService.DeleteUser((int)id);
            if (!isUserDeleted)
                ViewData["Message"] = "Error while deleting user";
            return RedirectToAction("List");
        }
    }
}
