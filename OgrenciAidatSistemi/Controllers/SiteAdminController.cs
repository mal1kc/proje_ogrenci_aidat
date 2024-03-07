using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Controllers
{
    // TODO: add policies https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-8.0
    public class SiteAdminController : Controller
    {
        private readonly ILogger<SiteAdminController> _logger;
        private readonly AppDbContext _appDbContext;

        private readonly ControllerHelper<SiteAdmin> _controllerHelper = new();

        public SiteAdminController(ILogger<SiteAdminController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, admin.EmailAddress),
                new Claim(ClaimTypes.Role, Configurations.Constants.userRoles.SiteAdmin),
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            };
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            // var authProperties = new AuthenticationProperties();
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
            return RedirectToAction("Index", "Home");
        }

        [
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            DebugOnly
        ]
        public IActionResult List(
            int pageIndex = 1,
            int pageSize = 10,
            string sortOrder = "updatedat_desc",
            string currentFilter = ""
        )
        {
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
                "updatedat_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.updatedAt),
                "createdat_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.createdAt),
                "firstname" => filteredSiteAdmins.OrderBy(admin => admin.FirstName),
                "lastname" => filteredSiteAdmins.OrderBy(admin => admin.LastName),
                "updatedat" => filteredSiteAdmins.OrderBy(admin => admin.updatedAt),
                "createdat" => filteredSiteAdmins.OrderBy(admin => admin.createdAt),
                _ => filteredSiteAdmins.OrderByDescending(admin => admin.updatedAt)
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
