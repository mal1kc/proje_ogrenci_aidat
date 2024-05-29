using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Helpers.Controller;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class ContactController(
        ILogger<ContactController> logger,
        AppDbContext dbContext,
        UserService userService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<ContactController> _logger = logger;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly UserService _userService = userService;

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
            searchField ??= "";
            searchString ??= "";
            sortOrder ??= "";
            return TryListOrFail(
                () =>
                    modelList.List(
                        ViewData,
                        searchString.ToSanitizedLowercase(),
                        searchField.ToSanitizedLowercase(),
                        sortOrder.ToSanitizedLowercase(),
                        pageIndex,
                        pageSize
                    ),
                "contact infos"
            );
        }

        // GET: Contact/Delete/5
        [DebugOnly]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Delete(int? id)
        {
            throw new NotImplementedException("Delete action not implemented");
        }

        // POST: Contact/Delete/5
        [DebugOnly]
        [
            HttpPost,
            ActionName("Delete"),
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin),
            ValidateAntiForgeryToken
        ]
        public IActionResult DeleteConfirmed(int? id)
        {
            throw new NotImplementedException("DeleteConfirmed action not implemented");
        }
    }
}
