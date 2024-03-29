using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;
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
            return View();
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
                await _userService.SignInUser(admin,UserRole.SiteAdmin);
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
        [HttpGet,
    Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
         DebugOnly
        ]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            if (_appDbContext.SiteAdmins == null)
            {
                _logger.LogError("SiteAdmins table is null");
                _appDbContext.SiteAdmins = _appDbContext.Set<SiteAdmin>();
            }

            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearchString"] = searchString;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            var modelSearch = new ModelSearch<SiteAdmin>(
                _appDbContext.SiteAdmins.AsQueryable(),
                new ModelSearchConfig(
                    SiteAdminSearchConfig.AllowedFieldsForSearch,
                    SiteAdminSearchConfig.AllowedFieldsForSort
                )
            );

            var filteredSiteAdmins = modelSearch.Search(searchString, searchField);

            Func<IQueryable<SiteAdmin>, IOrderedQueryable<SiteAdmin>> sortFunc = null;

            if (!string.IsNullOrEmpty(sortOrder))
            {
                string sortOrderBase = sortOrder.EndsWith("_desc")
                    ? sortOrder.Substring(0, sortOrder.Length - 5)
                    : sortOrder;

                if (!string.IsNullOrEmpty(searchField))
                {
                    if (!SiteAdminSearchConfig.AllowedFieldsForSort.Contains(sortOrderBase))
                        throw new ArgumentException($"Field '{sortOrderBase}' cannot be sorted.");
                }

                switch (sortOrderBase)
                {
                    case "Name":
                        sortFunc = q => q.OrderBy(e => e.FirstName).ThenBy(e => e.LastName);
                        break;
                    case "Date":
                        sortFunc = q => q.OrderBy(e => e.CreatedAt);
                        break;
                    // Add more sorting options for other fields as needed
                    default:
                        sortFunc = q => q.OrderBy(e => e.UpdatedAt); // default sort
                        break;
                }

                if (sortOrder.EndsWith("_desc"))
                {
                    sortFunc = q => sortFunc(q).OrderByDescending(e => e);
                }
            }

            var sortedSiteAdmins =
                sortFunc != null ? sortFunc(filteredSiteAdmins.AsQueryable()) : filteredSiteAdmins;

            var paginatedSiteAdmins = sortedSiteAdmins
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToList();

            ViewData["CurrentPageIndex"] = pageIndex;
            ViewData["TotalPages"] = (int)Math.Ceiling(filteredSiteAdmins.Count() / (double)pageSize);
            ViewData["TotalItems"] = filteredSiteAdmins.Count();
            ViewData["PageSize"] = pageSize;

            return View(paginatedSiteAdmins);
        }

        // var filteredSiteAdmins = _appDbContext.SiteAdmins.AsQueryable();
        // if (currentFilter is not "" and not null)
        // {
        //     filteredSiteAdmins = _appDbContext.SiteAdmins.Where(admin =>
        //         admin.FirstName.Contains(currentFilter) || string.IsNullOrEmpty(admin.LastName)
        //             ? true
        //             : admin.LastName.Contains(currentFilter)
        //     );
        // }
        // else
        // {
        //     currentFilter = "";
        //     filteredSiteAdmins = _appDbContext.SiteAdmins;
        // }

        // // create iterable of SiteAdmins with pagination and filtering, sorting


        // // switch case for sorting order
        // var sortedSiteAdmins = sortOrder switch
        // {
        //     "firstname_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.FirstName),
        //     "lastname_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.LastName),
        //     "updatedat_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.UpdatedAt),
        //     "createdat_desc" => filteredSiteAdmins.OrderByDescending(admin => admin.CreatedAt),
        //     "firstname" => filteredSiteAdmins.OrderBy(admin => admin.FirstName),
        //     "lastname" => filteredSiteAdmins.OrderBy(admin => admin.LastName),
        //     "updatedat" => filteredSiteAdmins.OrderBy(admin => admin.UpdatedAt),
        //     "createdat" => filteredSiteAdmins.OrderBy(admin => admin.CreatedAt),
        //     _ => filteredSiteAdmins.OrderByDescending(admin => admin.UpdatedAt)
        // };

        // var paginatedSiteAdmins = filteredSiteAdmins
        //     .Skip((pageIndex - 1) * pageSize)
        //     .Take(pageSize);

        // ViewData["CurrentSort"] = sortOrder;
        // ViewData["CurrentFilter"] = currentFilter;

        // return View(paginatedSiteAdmins);
        //       }

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
