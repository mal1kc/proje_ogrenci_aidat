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

            /* ViewData["CurrentSortOrder"] = sortOrder; */
            /* ViewData["CurrentSearchString"] = searchString; */
            /* ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : ""; */
            /* ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date"; */
            /**/
            /* var modelSearch = new QueryableModelHelper<SchoolAdmin>( */
            /*         _dbContext.SchoolAdmins.AsQueryable(), */
            /*         new ModelSearchConfig( */
            /*             SchoolAdminSearchConfig.AllowedFieldsForSearch, */
            /*             SchoolAdminSearchConfig.AllowedFieldsForSort */
            /*         ) */
            /*         ); */
            /**/
            /* IQueryable<SchoolAdmin> modelOpResult = modelSearch.SearchAndSort( */
            /*     searchString, */
            /*     searchField, */
            /*     sortOrder */
            /* ); */
            /**/
            /* Console.WriteLine("modelOpResult: {0}", modelOpResult.Count()); */
            /**/
            /* var paginatedModel = modelOpResult */
            /* .Skip((pageIndex - 1) * pageSize) */
            /* .Take(pageSize) */
            /*     .AsQueryable(); */
            /**/
            /* var countOfmodelopResult = modelOpResult.Count(); */
            /**/
            /**/
            /* ViewData["CurrentPageIndex"] = pageIndex; */
            /* ViewData["TotalPages"] = (int) */
            /*     Math.Ceiling(countOfmodelopResult / (double)pageSize); */
            /* ViewData["TotalItems"] = countOfmodelopResult; */
            /* ViewData["PageSize"] = pageSize; */
            /**/
            /* Console.WriteLine( */
            /*     "ids of paginateSchAdmins: {0}", */
            /*     string.Join(", ", paginatedModel.Select(e => e.Id)) */
            /* ); */
            /**/
            /* return View(paginatedModel); */
        }
    }
}
