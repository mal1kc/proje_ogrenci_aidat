using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Controllers
{
    public class WorkYearController : Controller
    {
        private readonly ILogger<WorkYearController> _logger;

        private readonly AppDbContext _dbContext;

        public WorkYearController(ILogger<WorkYearController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

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
            #region get user role and id
            string? stringRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            int? usrId = null;
            string nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (stringRole == null || nameIdentifier == null)
            {
                _logger.LogError("User role is null");
                return RedirectToAction("Index", "Home");
            }
            usrId = int.Parse(nameIdentifier);
            UserRole userRole = UserRoleExtensions.GetRoleFromString(stringRole);
            #endregion

            #region check user role and get data owned by user
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    break;
                case UserRole.SchoolAdmin:
                    var schadmin = _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefault(sa => sa.Id == usrId);
                    if (schadmin == null)
                    {
                        _logger.LogError("SchoolAdmin is null");
                        return RedirectToAction("Index", "Home");
                    }

                    var modelList2 = new QueryableModelHelper<WorkYear>(
                        _dbContext
                            .WorkYears.Where(wy => wy.School.Id == schadmin.School.Id)
                            .Include(wy => wy.School)
                            .Include(wy => wy.PaymentPeriods),
                        WorkYear.SearchConfig
                    );
                    return View(
                        modelList2.List(
                            ViewData,
                            searchString,
                            searchField,
                            sortOrder,
                            pageIndex,
                            pageSize
                        )
                    );
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }
            #endregion

            if (_dbContext.WorkYears == null)
            {
                _logger.LogError("WorkYears table is null");
                _dbContext.WorkYears = _dbContext.Set<WorkYear>();
            }
            var modelList = new QueryableModelHelper<WorkYear>(
                _dbContext
                    .WorkYears.Include(wy => wy.PaymentPeriods)
                    .Include(wy => wy.School)
                    .AsQueryable(),
                WorkYear.SearchConfig
            );
            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
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
            // TODO: user schooladminservice to get schooladmin's data
            #region get user role and id
            string? stringRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            int? usrId = null;
            string nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (stringRole == null || nameIdentifier == null)
            {
                _logger.LogError("User role is null");
                return RedirectToAction("Index", "Home");
            }
            usrId = int.Parse(nameIdentifier);
            UserRole userRole = UserRoleExtensions.GetRoleFromString(stringRole);
            #endregion
            #region check user role and get data owned by user
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    break;
                case UserRole.SchoolAdmin:
                    var schadmin = _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefault(sa => sa.Id == usrId);
                    if (schadmin == null)
                    {
                        _logger.LogError("SchoolAdmin is null");
                        return RedirectToAction("Index", "Home");
                    }
                    var sch_workYear = _dbContext
                        .WorkYears.Where(wy => wy.Id == id && wy.School.Id == schadmin.School.Id)
                        .Include(wy => wy.PaymentPeriods)
                        .Include(wy => wy.School)
                        .FirstOrDefault();
                    if (sch_workYear == null)
                    {
                        _logger.LogInformation("Could not find WorkYear with id " + id);
                        TempData["Message"] = "Could not find WorkYear with id " + id;
                        return RedirectToAction("List");
                    }
                    return View(sch_workYear.ToView());
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }
            #endregion

            if (id == null)
            {
                _logger.LogError("WorkYear id is null");
                TempData["Error"] = "WorkYear id is null";
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

        [HttpPost]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                _logger.LogError("WorkYear id is null");
                TempData["Error"] = "WorkYear id is null";
                return RedirectToAction("List");
            }
            #region get user role and id
            string? stringRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            int? usrId = null;
            string nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (stringRole == null || nameIdentifier == null)
            {
                _logger.LogError("User role is null");
                return RedirectToAction("Index", "Home");
            }
            usrId = int.Parse(nameIdentifier);
            UserRole userRole = UserRoleExtensions.GetRoleFromString(stringRole);
            #endregion
            #region check user role and get data owned by user

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
                    var schadmin = _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefault(sa => sa.Id == usrId);
                    if (schadmin == null)
                    {
                        _logger.LogError("SchoolAdmin is null");
                        return RedirectToAction("Index", "Home");
                    }
                    workYear = _dbContext
                        .WorkYears.Where(wy => wy.Id == id && wy.School.Id == schadmin.School.Id)
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
            _dbContext.WorkYears.Remove(workYear);
            _dbContext.SaveChanges();
            TempData["Message"] = "WorkYear deleted successfully";
            return RedirectToAction("List");
        }

        [HttpGet]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult Create()
        {
            #region get user role and id
            string? stringRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            int? usrId = null;
            string nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (stringRole == null || nameIdentifier == null)
            {
                _logger.LogError("User role is null");
                return RedirectToAction("Index", "Home");
            }
            usrId = int.Parse(nameIdentifier);
            UserRole userRole = UserRoleExtensions.GetRoleFromString(stringRole);
            #endregion
            #region check user role and get data owned by user

            IQueryable<School>? schools = null;

            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    schools = _dbContext.Schools.AsQueryable();
                    break;
                case UserRole.SchoolAdmin:
                    var schadmin = _dbContext
                        .SchoolAdmins.Include(sa => sa.School)
                        .FirstOrDefault(sa => sa.Id == usrId);
                    if (schadmin == null)
                    {
                        _logger.LogError("SchoolAdmin is null");
                        return RedirectToAction("Index", "Home");
                    }
                    schools = _dbContext.Schools.Where(s => s.Id == schadmin.School.Id);
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }
            #endregion
            ViewBag.Schools = schools;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = Constants.userRoles.SiteAdmin + "," + Constants.userRoles.SchoolAdmin)]
        public IActionResult Create(WorkYearView workYear)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "WorkYear is not valid";
                return RedirectToAction("Create");
            }

            // TODO: use schooladminservice to check if is signed schadmin's school and workyear's school is same

            var school = _dbContext.Schools.FirstOrDefault(s => s.Id == workYear.SchoolId);
            if (school == null)
            {
                TempData["Error"] = "School does not exist";
                return RedirectToAction("Create");
            }
            var workYearModel = new WorkYear
            {
                StartDate = workYear.StartDate,
                EndDate = workYear.EndDate,
                School = school
            };

            _dbContext.WorkYears.Add(workYearModel);

            return View(workYear);
        }
    }
}
