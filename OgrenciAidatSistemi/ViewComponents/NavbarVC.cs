using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;
#pragma warning disable 8604
namespace OgrenciAidatSistemi.ViewComponents
{
    public class NavbarVC(ILogger<NavbarVC> logger, UserService userService) : ViewComponent
    {
        private readonly ILogger<NavbarVC> _logger = logger;
        private readonly UserService _userService = userService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var usr = await _userService.GetCurrentUserAsync();

            if (usr != null)
            {
                try
                {
                    var userView = usr.ToView();
                    ViewBag.User = userView;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while converting user to view");
                }
            }
            return View();
        }
    }
}
