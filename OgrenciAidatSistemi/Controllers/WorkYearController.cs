using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class WorkYearController(
        ILogger<WorkYearController> logger,
        AppDbContext dbContext,
        UserService userService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<WorkYearController> _logger = logger;

        private readonly AppDbContext _dbContext = dbContext;

        private readonly UserService _userService = userService;

        [HttpGet]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
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

            ViewBag.IsSiteAdmin = false;
            var (userRole, schId) = _userService.GetUserRoleAndSchoolId().Result;

            var workYears = _dbContext
                .WorkYears.Include(wy => wy.PaymentPeriods)
                .Include(wy => wy.School)
                .AsQueryable();
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    break;
                case UserRole.SchoolAdmin:
                    if (schId == null || schId == 0 || schId < 0)
                    {
                        _logger.LogError("School id is null");
                        return RedirectToAction("Index", "Home");
                    }
                    workYears = workYears
                        .Where(wy => wy.School != null && wy.School.Id == schId)
                        .AsQueryable();
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }

            var modelList = new QueryableModelHelper<WorkYear>(workYears, WorkYear.SearchConfig);

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
                "work years"
            );
        }

        [HttpGet]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                _logger.LogError("WorkYear id is null");
                TempData["Error"] = "WorkYear id is null";
                return RedirectToAction("List");
            }
            if (_dbContext.WorkYears == null)
            {
                _logger.LogError("WorkYears table is null");
                TempData["Error"] = "could not find WorkYear with id " + id;
                return RedirectToAction("List");
            }
            var workYear = _dbContext
                .WorkYears.Where(wy => wy.Id == id)
                .Include(wy => wy.PaymentPeriods)
                .Include(wy => wy.School)
                .FirstOrDefault();

            if (workYear == null)
            {
                _logger.LogInformation("Could not find WorkYear with id " + id);
                TempData["Message"] = "Could not find WorkYear with id " + id;
                return RedirectToAction("List");
            }
            return View(workYear.ToView());
        }

        [HttpGet]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogError("WorkYear id is null");
                TempData["Error"] = "WorkYear id is null";
                return RedirectToAction("List");
            }
            #region get user role and schoolId from claims to get user data

            var (userRole, schId) = _userService.GetUserRoleAndSchoolId().Result;
            WorkYear? workYear = null;
            if (_dbContext.WorkYears == null)
            {
                _logger.LogError("WorkYears table is null");
                TempData["Error"] = "could not find WorkYear with id " + id;
                return RedirectToAction("List");
            }

            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    workYear = _dbContext
                        .WorkYears.Where(wy => wy.Id == id)
                        .Include(wy => wy.PaymentPeriods)
                        .Include(wy => wy.School)
                        .FirstOrDefault();
                    break;
                case UserRole.SchoolAdmin:
                    if (schId == null || schId == 0 || schId < 0)
                    {
                        _logger.LogError("School id is null");
                        return RedirectToAction("Index", "Home");
                    }

                    workYear = _dbContext
                        .WorkYears.Where(wy =>
                            wy.Id == id && wy.School != null && wy.School.Id == schId
                        )
                        .Include(wy => wy.PaymentPeriods)
                        .Include(wy => wy.School)
                        .FirstOrDefault();
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }
            #endregion
            if (workYear == null)
            {
                _logger.LogInformation("Could not find WorkYear with id " + id);
                TempData["Message"] = "Could not find WorkYear with id " + id;
                return RedirectToAction("List");
            }

            try
            {
                return View(workYear.ToView());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in WorkYearController.Delete");
                TempData["Error"] = "We encountered an error while deleting WorkYear";
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                _logger.LogError("WorkYear id is null");
                TempData["Error"] = "WorkYear id is null";
                return RedirectToAction("List");
            }
            if (_dbContext.WorkYears == null)
            {
                _logger.LogError("WorkYears table is null");
                TempData["Error"] = "could not find WorkYear with id " + id;
                return RedirectToAction("List");
            }
            #region get user role and schoolId from claims to get user data
            var (userRole, usrId) = _userService.GetUserRoleAndSchoolId().Result;

            WorkYear? workYear = null;
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    workYear = _dbContext
                        .WorkYears.Where(wy => wy.Id == id)
                        .Include(wy => wy.PaymentPeriods)
                        .Include(wy => wy.School)
                        .FirstOrDefault();
                    break;
                case UserRole.SchoolAdmin:
                    workYear = _dbContext
                        .WorkYears.Where(wy =>
                            wy.Id == id && wy.School != null && wy.School.Id == usrId
                        )
                        .Include(wy => wy.PaymentPeriods)
                        .Include(wy => wy.School)
                        .FirstOrDefault();
                    if (workYear == null)
                    {
                        _logger.LogInformation("Could not find WorkYear with id " + id);
                        TempData["Message"] = "Could not find WorkYear with id " + id;
                        return RedirectToAction("List");
                    }
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }
            #endregion



            if (workYear == null)
            {
                _logger.LogInformation("Could not find WorkYear with id " + id);
                TempData["Message"] = "Could not find WorkYear with id " + id;
                return RedirectToAction("List");
            }
            try
            {
                _dbContext.WorkYears.Remove(workYear);
                _dbContext.SaveChanges();
                TempData["Message"] = "WorkYear deleted successfully";
                return RedirectToAction("List");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in WorkYearController.DeleteConfirmed");
                TempData["Error"] = "We encountered an error while deleting WorkYear";
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult Create()
        {
            ViewBag.IsSiteAdmin = false;
            if (_dbContext.Schools == null)
            {
                _logger.LogError("Schools table is null");
                TempData["Error"] = "Schools table is null";
                return RedirectToAction("List");
            }
            IQueryable<School>? schools = _dbContext.Schools;

            var (userRole, usrId) = _userService.GetUserRoleAndSchoolId().Result;
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    break;
                case UserRole.SchoolAdmin:
                    schools = schools.Where(s => s.Id == usrId).AsQueryable();
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }
            ViewBag.Schools = schools.AsQueryable();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public async Task<IActionResult> Create(WorkYearView workYear)
        {
            var (userRole, schID) = await _userService.GetUserRoleAndSchoolId();
            var schools = _dbContext.Schools.AsQueryable();
            if (userRole == UserRole.SchoolAdmin)
            {
                schools = schools.Where(s => s.Id == schID && schID != null);
            }
            ViewBag.Schools = schools;

            if (workYear.StartDate > workYear.EndDate)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be greater than end date");
                return View(workYear);
            }
            var fifteendaysAgo = DateOnly.FromDateTime(DateTime.UtcNow - TimeSpan.FromDays(15));
            if (workYear.StartDate < fifteendaysAgo)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be less than 15 days ago");
                return View(workYear);
            }

            if (workYear.SchoolId <= 0 || workYear.SchoolId == null)
            {
                ModelState.AddModelError("SchoolId", "School is required");
                return View(workYear);
            }

            School? school = null;

            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    school = await _dbContext.Schools.FirstOrDefaultAsync(s =>
                        s.Id == workYear.SchoolId
                    );
                    if (school == null)
                    {
                        ViewBag.Schools = await _dbContext.Schools.ToListAsync();
                        return View(workYear);
                    }
                    break;
                case UserRole.SchoolAdmin:
                    if (schID == null || schID == 0 || schID < 0)
                    {
                        _logger.LogError("School id is null");
                        return RedirectToAction("Index", "Home");
                    }
                    school = await _dbContext.Schools.FirstOrDefaultAsync(s => s.Id == schID);
                    if (school == null)
                    {
                        await _userService.SignOutUser();
                        TempData["Error"] = "School does not exist";
                        return RedirectToAction("Index", "Home");
                    }
                    if (workYear.SchoolId != schID)
                    {
                        ViewBag.Schools = await _dbContext.Schools.ToListAsync();
                        return View(workYear);
                    }
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }

            var workYearModel = new WorkYear
            {
                StartDate = workYear.StartDate,
                EndDate = workYear.EndDate,
                School = school
            };

            _dbContext.WorkYears.Add(workYearModel);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("List");
        }
    }
}
