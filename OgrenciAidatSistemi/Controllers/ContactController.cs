using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    // TODO: change authorization roles for actions
    // must be SiteAdmin or SchoolAdmin (for its school and school students) => list, create, edit, delete ,details
    // or Student (for its own contact info) => details, edit

    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly AppDbContext _dbContext;
        private readonly UserService _userService;

        public ContactController(
            ILogger<ContactController> logger,
            AppDbContext dbContext,
            UserService userService
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _userService = userService;
        }

#warning "This action not tested"
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            if (_dbContext.Contacts == null)
            {
                _logger.LogError("Contacts table is null");
                _dbContext.Contacts = _dbContext.Set<ContactInfo>();
            }
            var modelList = new QueryableModelHelper<ContactInfo>(
                _dbContext.Contacts.AsQueryable(),
                ContactInfo.SearchConfig
            );
            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
            );
        }

#warning "This action not tested"

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Contacts = _dbContext.Contacts;
            return View();
        }

        // POST: Contact/Create

        [HttpPost]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContactInfo model)
        {
            // TODO:: implement contactinfo create action
            throw new NotImplementedException("Create action not implemented");
        }

        // GET: Contact/Delete/5
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Delete(int? id)
        {
            // TODO: impelment contactinfo delete action
            throw new NotImplementedException("Delete action not implemented");
        }

        // POST: Contact/Delete/5
        [
            HttpPost,
            ActionName("Delete"),
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)
        ]
        public IActionResult DeleteConfirmed(int? id)
        {
            // TODO: implement contactinfo delete action
            throw new NotImplementedException("DeleteConfirmed action not implemented");
        }

        // GET: Contact/Detais/5

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Details(int? id)
        {
            // TODO: implement contactinfo details action
            throw new NotImplementedException("Details action not implemented");
        }
    }
}
