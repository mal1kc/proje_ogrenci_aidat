using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public SchoolController(ILogger<SchoolController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SchoolAdmin)]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult List(
            string searchString = null,
            string searchField = null,
            string sortOrder = null,
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
    }
}
