using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class SiteAdminController : Controller
    {
        private readonly ILogger<SiteAdminController> _logger;
        private readonly AppDbContext _appDbContext;

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
            if (_userService.IsUserSignedIn())
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
                await _userService.SignInUser(admin, UserRole.SiteAdmin);
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
            if (_appDbContext.SiteAdmins == null)
            {
                _logger.LogError("SiteAdmins table is null");
                _appDbContext.SiteAdmins = _appDbContext.Set<SiteAdmin>();
            }

            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearchString"] = searchString;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            var modelSearch = new QueryableModelHelper<SiteAdmin>(
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
                string sortOrderBase = sortOrder.ToLower().EndsWith("_desc")
                    ? sortOrder.Substring(0, sortOrder.Length - 5)
                    : sortOrder;

                if (!string.IsNullOrEmpty(searchField))
                {
                    if (!SiteAdminSearchConfig.AllowedFieldsForSearch.Contains(searchField))
                        throw new ArgumentException("Invalid search field");
                }

                switch (sortOrderBase)
                {
                    case "name":
                        sortFunc = q => q.OrderBy(e => e.FirstName).ThenBy(e => e.LastName);
                        break;
                    case "date":
                        sortFunc = q => q.OrderBy(e => e.CreatedAt);
                        break;
                    // Add more sorting options for other fields as needed
                    default:
                        sortFunc = q => q.OrderBy(e => e.UpdatedAt); // default sort
                        break;
                }

                // Apply descending order if needed
                if (sortOrder.EndsWith("_desc"))
                {
                    var sortedQuery = sortFunc(filteredSiteAdmins.AsQueryable());
                    sortFunc = q => sortedQuery.OrderByDescending(e => e);
                }
            }

            Console.WriteLine("After determine Sorting Func {0} with {1}", sortFunc, sortOrder);
            // call the sort function
            var sortedSiteAdmins =
                sortFunc != null ? sortFunc(filteredSiteAdmins.AsQueryable()) : filteredSiteAdmins;
            Console.WriteLine("After Sorting");

            var paginatedSiteAdmins = sortedSiteAdmins
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsQueryable();

            ViewData["CurrentPageIndex"] = pageIndex;
            ViewData["TotalPages"] = (int)
                Math.Ceiling(filteredSiteAdmins.Count() / (double)pageSize);
            ViewData["TotalItems"] = filteredSiteAdmins.Count();
            ViewData["PageSize"] = pageSize;

            Console.WriteLine(
                "ids of paginatedsiteadmins: {0}",
                string.Join(", ", paginatedSiteAdmins.Select(e => e.Id))
            );

            return View(paginatedSiteAdmins);

            // create collecttion from paginatedsiteadmins with ids

            var paginatedSiteAdmins_withIds = _appDbContext
                .SiteAdmins.Join(
                    _appDbContext.Users,
                    sa => sa.Id, // Foreign key property in SiteAdmin
                    ot => ot.Id, // Primary key property in OtherTable
                    (sa, ot) =>
                        new SiteAdmin
                        {
                            Id = ot.Id,
                            FirstName = sa.FirstName,
                            LastName = sa.LastName,
                            Username = sa.Username,
                            EmailAddress = sa.EmailAddress,
                            PasswordHash = sa.PasswordHash,
                            CreatedAt = sa.CreatedAt,
                            UpdatedAt = sa.UpdatedAt
                        }
                )
                .ToList();
            return View(paginatedSiteAdmins_withIds);
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

                SiteAdmin newSiteAdmin = new SiteAdmin
                {
                    Username = siteAdmin.Username,
                    PasswordHash = SiteAdmin.ComputeHash(siteAdmin.Password),
                    EmailAddress = siteAdmin.EmailAddress,
                    FirstName = siteAdmin.FirstName,
                    LastName = siteAdmin.LastName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _appDbContext.SiteAdmins.Add(newSiteAdmin);
                await _appDbContext.SaveChangesAsync();
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siteAdmin = _appDbContext.SiteAdmins.Where(e => e.Id == id).FirstOrDefault();

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


        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        [DebugOnly]
        // [DisabledAction]

        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            /* var siteAdmin = _appDbContext.SiteAdmins.Where(e => e.Id == id).FirstOrDefault(); */
            /**/

            var siteAdminUser = _appDbContext.Users.Where(e => e.Id == id).FirstOrDefault();

            if (siteAdminUser == null)
            {
                return NotFound();
            }

            bool isDeleted = await _userService.DeleteUser(siteAdminUser.Id);
            if (!isDeleted)
                ViewData["Message"] = "Error while deleting the user";
            return RedirectToAction("List");
        }
    }
}
