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
    public class SchoolController(
        ILogger<SchoolController> logger,
        AppDbContext dbContext,
        UserService userService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<SchoolController> _logger = logger;

        private readonly AppDbContext _dbContext = dbContext;

        private readonly UserService _userService = userService;

        [Authorize(Roles = Configurations.Constants.userRoles.SchoolAdmin)]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
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
            var modelList = new QueryableModelHelper<School>(
                _dbContext.Schools.AsQueryable(),
                School.SearchConfig
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
                "schools"
            );
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Create(SchoolView schoolView)
        {
            if (ModelState.IsValid)
            {
                if (schoolView == null)
                {
                    _logger.LogError("SchoolView is null");
                    return NotFound();
                }

                if (schoolView.Name == null)
                {
                    _logger.LogError("SchoolView.Name is null");
                    TempData["Error"] = "School name cannot be empty";
                    return RedirectToAction("Create");
                }

                var school = new School
                {
                    Name = schoolView.Name,
                    Students = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Schools.Add(school);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("List");
            }

            return View(schoolView);
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0 || _dbContext.Schools == null)
                return NotFound();
            School? school = null;

            var signedUser = await _userService.GetCurrentUserAsync();
            if (signedUser == null)
                return Unauthorized();

            _logger.LogInformation("Signed user: {0}, {1}", signedUser.Id, signedUser.Role);

            switch (signedUser.Role)
            {
                case UserRole.SchoolAdmin:
                    school = await _dbContext
                        .Schools.Include(s => s.Students)
                        .Include(s => s.SchoolAdmins)
                        .Where(s => s.Id == id)
                        .FirstOrDefaultAsync();
                    if (school == null)
                        return NotFound();
                    if (school.Id != id)
                        return Unauthorized();
                    break;
                case UserRole.SiteAdmin:
                    school = await _dbContext
                        .Schools.Include(s => s.Students)
                        .Include(s => s.SchoolAdmins)
                        .Where(s => s.Id == id)
                        .FirstOrDefaultAsync();
                    if (school == null)
                        return NotFound();
                    break;
                default:
                    return Unauthorized();
            }

            return View(school.ToView());
        }

        // HTTP GET: School/Delete/5

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (_dbContext.Schools == null)
            {
                _logger.LogError("Schools table is null");
                return NotFound();
            }

            var school = await _dbContext.Schools.FindAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            return View(school.ToView());
        }

        // HTTP POST: School/DeleteConfirmed/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_dbContext.Schools == null)
            {
                _logger.LogError("Schools table is null");
                return NotFound();
            }

            var school = await _dbContext.Schools.FindAsync(id);
            if (school == null)
                return NotFound();

            try
            {
                // TODO: implement backup
                // create backup of all related data before deleting
                School? related = _dbContext.Schools.Where(s => s.Id == id).FirstOrDefault();

                if (related == null)
                    return NotFound();

                _dbContext.Schools.Remove(related);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Error while deleting school: {0}", e.Message);
                if (e.InnerException != null)
                    _logger.LogError("Inner exception: {0}", e.InnerException.Message);
                TempData["Error"] = "Error while deleting school";
                return RedirectToAction("Delete", new { id });
            }

            return RedirectToAction("List");
        }
    }
}
