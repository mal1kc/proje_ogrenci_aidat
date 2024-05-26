using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class UserCommonController : Controller
    {
        private readonly ILogger<UserCommonController> _logger;
        private readonly AppDbContext _appDbContext;

        private readonly UserService _userService;

        public UserCommonController(
            ILogger<UserCommonController> logger,
            AppDbContext appDbContext,
            UserService userService
        )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _userService = userService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SignOutUser()
        {
            if (await _userService.IsUserSignedIn())
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
