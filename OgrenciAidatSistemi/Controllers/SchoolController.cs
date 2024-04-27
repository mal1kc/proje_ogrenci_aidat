using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class SchoolController : Controller
    {
        private readonly ILogger<SchoolController> _logger;

        private readonly AppDbContext _dbContext;


        private readonly UserService _userService;

        public SchoolController(ILogger<SchoolController> logger, AppDbContext dbContext)
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

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            if (_dbContext.Schools == null)
            {
                _logger.LogError("Schools table is null");
                _dbContext.Schools = _dbContext.Set<School>();
            }

            var modelList = new QueryableModelHelper<School>(
                _dbContext.Schools.AsQueryable(),
                new ModelSearchConfig(
                    SchoolSearchConfig.AllowedFieldsForSearch,
                    SchoolSearchConfig.AllowedFieldsForSort
                )
            );

            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
            );
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Schools = _dbContext.Schools;
            return View();
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

            if (_dbContext.Schools == null)
            {
                _logger.LogError("Schools table is null");
                _dbContext.Schools = _dbContext.Set<School>();
            }

            var school = await _dbContext.Schools.FindAsync(id);
            if (school == null)
            {
                return NotFound();
            }
            _dbContext.Schools.Remove(school);

            return RedirectToAction("List");
        }

        // HTTP GET: School/Details/5
        // can be accessed by school admin, site admin



        [Authorize(Roles = Configurations.Constants.userRoles.SchoolAdmin + "," + Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0 || _dbContext.Schools == null)
                return NotFound();


            if (id == null || id == 0 || _dbContext.Schools == null)
                return NotFound();

            // if is schadmin check schadmin's school then continue
            // else if site admin continue
            // else return unauthorized
            //
            School? school = null;

            var signedUser = await _userService.GetCurrentUser();
            if (signedUser == null)
                return Unauthorized();

            _logger.LogInformation("Signed user: {0}, {1}", signedUser.Id, signedUser.Role);

            if (signedUser.Role == UserRole.SchoolAdmin)
            {
                school = await _dbContext.Schools.Where(s => s.Id == id).FirstOrDefaultAsync();
                if (school == null)
                    return NotFound();
                if (school.Id != id)
                    return Unauthorized();
            }
            else if (signedUser.Role == UserRole.SiteAdmin)
            {
                school = await _dbContext.Schools.Where(s => s.Id == id).FirstOrDefaultAsync();
                if (school == null)
                    return NotFound();
            }
            else
            {
                return Unauthorized();
            }

            return View(school.ToView());
        }

    }
}
