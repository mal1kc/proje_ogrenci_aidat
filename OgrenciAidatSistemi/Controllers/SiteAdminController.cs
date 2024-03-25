using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    // TODO: add policies https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-8.0
    public class SiteAdminController : Controller
    {
        private readonly ILogger<SiteAdminController> _logger;
        private readonly AppDbContext _appDbContext;

        private readonly ControllerHelper<SiteAdmin> _controllerHelper = new();

        private readonly UserService _userService;

        public SiteAdminController(ILogger<SiteAdminController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _userService = new UserService(appDbContext, new HttpContextAccessor());
        }

        // AKA : Admin Dashboard
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "AdminDashboard");
        }

        [HttpGet(Configurations.Constants.AdminAuthenticationLoginPath)]
        public ActionResult SignIn()
        {
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
            if (_appDbContext.SiteAdmins == null)
            {
                _logger.LogError("SiteAdmins table is null");
                _appDbContext.SiteAdmins = _appDbContext.Set<SiteAdmin>();
            }
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
                await _userService.SignInUser(admin);
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

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin), DebugOnly]
        public IActionResult List(
            int pageIndex = 1,
            int pageSize = 10,
            string sortOrder = "updatedat_desc",
            string currentFilter = ""
        )
        {
            if (_appDbContext.SiteAdmins == null)
            {
                _logger.LogError("SiteAdmins table is null");
                _appDbContext.SiteAdmins = _appDbContext.Set<SiteAdmin>();
            }

            var filteredSiteAdmins = _appDbContext.SiteAdmins.AsQueryable();
            if (currentFilter is not "" and not null)
            {
                filteredSiteAdmins = _appDbContext.SiteAdmins.Where(admin =>
                    admin.FirstName.Contains(currentFilter) || string.IsNullOrEmpty(admin.LastName)
                        ? true
                        : admin.LastName.Contains(currentFilter)
                );
            }
            else
            {
                currentFilter = "";
                filteredSiteAdmins = _appDbContext.SiteAdmins;
            }

            // create iterable of SiteAdmins with pagination and filtering, sorting


            // switch case for sorting order
            var sortedSiteAdmins = sortOrder switch
            {
                "firstname_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.FirstName),
                "lastname_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.LastName),
                "updatedat_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.UpdatedAt),
                "createdat_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.CreatedAt),
                "firstname" => filteredSiteAdmins.OrderBy(admin => admin.FirstName),
                "lastname" => filteredSiteAdmins.OrderBy(admin => admin.LastName),
                "updatedat" => filteredSiteAdmins.OrderBy(admin => admin.UpdatedAt),
                "createdat" => filteredSiteAdmins.OrderBy(admin => admin.CreatedAt),
                _ => filteredSiteAdmins.OrderByDescending(admin => admin.UpdatedAt)
            };

            var paginatedSiteAdmins = filteredSiteAdmins
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = currentFilter;

            return View(paginatedSiteAdmins);
        }

        [DebugOnly]
        public string[] GenerateRandomNames()
        {
            // generate random sized array of strings
            // string lengh between 5 and 21

            Random random = new Random();
            int arraySize = random.Next(5, 21);
            string[] names = new string[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                names[i] = Path.GetRandomFileName().Replace(".", "");
            }
            return names;
        }


    }
}
