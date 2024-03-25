using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers{

    public class UserCommonController : Controller
    {
        private readonly ILogger<UserCommonController> _logger;
        private readonly AppDbContext _appDbContext;

        private readonly ControllerHelper<User> _controllerHelper = new();

        private readonly UserService _userService;

        public UserCommonController(ILogger<UserCommonController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _userService = new UserService(appDbContext, new HttpContextAccessor());
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SignOutUser()
        {
            // if user is not signed in, redirect to home page
            // else sign out user and redirect to home page with logged out message

            if (_userService.IsUserSignedIn())
            {
                _logger.LogInformation("SignOutUser() -> user is signed in, signing out");
                await _userService.SignOutUser();
                TempData["LoggedOut"] = true;
                TempData["Message"] = "You have been logged out";
            }
            _logger.LogInformation("SignOutUser() -> redirecting to home page");
            return RedirectToAction("Index", "Home");
        }

    }
}
