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
    ) : Controller
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
            ViewBag.IsSiteAdmin = false;
            #region get user role and groupSid from claims to get user data

            var (userRole, schId) = _userService.GetUserRoleAndSchoolId().Result;

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

                    var modelList2 = new QueryableModelHelper<WorkYear>(
                        _dbContext
                            .WorkYears.Where(wy => wy.School != null && wy.School.Id == schId)
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
            IQueryable<School>? schools = null;

            #region get user role and schoolId from claims to get user data

            var (userRole, usrId) = _userService.GetUserRoleAndSchoolId().Result;
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    schools = _dbContext.Schools.AsQueryable();
                    break;
                case UserRole.SchoolAdmin:
                    schools = _dbContext.Schools.Where(s => s.Id == usrId).AsQueryable();
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




            School? school = null;

            var (userRole, schID) = _userService.GetUserRoleAndSchoolId().Result;
            switch (userRole)
            {
                case UserRole.SiteAdmin:
                    school = _dbContext.Schools.FirstOrDefault(s => s.Id == workYear.SchoolId);
                    if (school == null)
                    {
                        TempData["Error"] = "School does not exist";
                        return RedirectToAction("Create");
                    }
                    break;
                case UserRole.SchoolAdmin:
                    if (schID == null || schID == 0 || schID < 0)
                    {
                        _logger.LogError("School id is null");
                        return RedirectToAction("Index", "Home");
                    }
                    school = _dbContext.Schools.FirstOrDefault(s => s.Id == schID);
                    if (school == null)
                    {
                        _userService.SignOutUser().RunSynchronously();
                        TempData["Error"] = "School does not exist";
                        return RedirectToAction("Index", "Home");
                    }
                    break;
                default:
                    _logger.LogError("User role is not valid");
                    return RedirectToAction("Index", "Home");
            }

            if (school == null || (workYear.School != null && workYear.School.Id != school.Id))
            {
                TempData["Error"] = "You are not authorized to create WorkYear for this school";
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
