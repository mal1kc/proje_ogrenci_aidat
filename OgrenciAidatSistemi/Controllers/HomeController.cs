using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class HomeController(ILogger<HomeController> logger, UserService userService)
        : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly UserService _userService = userService;

        public async Task<IActionResult> Index()
        {
            // if is authenticated, redirect to dashboard of their role
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var (userRole, _) = await _userService.GetUserRoleAndSchoolId();
                return userRole switch
                {
                    UserRole.SchoolAdmin => RedirectToAction("Index", "SchoolAdmin"),
                    UserRole.Student => RedirectToAction("Index", "Student"),
                    UserRole.SiteAdmin => RedirectToAction("Index", "SiteAdmin"),
                    _ => RedirectToAction("Login"),
                };
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
