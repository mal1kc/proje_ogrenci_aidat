using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
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
        
    }
}
