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

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            var modelList = new QueryableModelHelper<ContactInfo>(
                _dbContext.Contacts.AsQueryable(),
                ContactInfo.SearchConfig
            );
            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
            );
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
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            ValidateAntiForgeryToken
        ]
        public IActionResult DeleteConfirmed(int? id)
        {
            // TODO: implement contactinfo delete action
            throw new NotImplementedException("DeleteConfirmed action not implemented");
        }
    }
}
