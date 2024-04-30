using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;
#pragma warning disable 8604
namespace OgrenciAidatSistemi.ViewComponents
{
    public class NavbarVC : ViewComponent
    {
        private readonly ILogger<NavbarVC> _logger;
        private readonly AppDbContext _appDbContext;

        public NavbarVC(ILogger<NavbarVC> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public IViewComponentResult Invoke()
        {
            if (HttpContext.User.Claims.Any())
            {
                String? claimEmail = HttpContext
                    .User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)
                    ?.Value;

                if (claimEmail == null)
                {
                    return View(null);
                }
                String? claimRole = HttpContext
                    .User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)
                    ?.Value;

                return claimRole switch
                {
                    "SiteAdminR" => InvokeSiteAdmin(claimEmail),
                    "StudentR" => InvokeStudent(claimEmail),
                    _ => View(null)
                };
            }

            return View(null);
        }

        public IViewComponentResult InvokeSiteAdmin(String claimEmail)
        {
            SiteAdmin? admin = _appDbContext
                .SiteAdmins.Where(_admin => _admin.EmailAddress == claimEmail)
                .FirstOrDefault();
            ViewBag.User = admin;
            return View();
        }

        public IViewComponentResult InvokeStudent(String claimEmail)
        {
            Student? student = _appDbContext
                .Students.Where(_student => _student.EmailAddress == claimEmail)
                .FirstOrDefault();
            ViewBag.User = student;
            return View();
        }

        public IViewComponentResult InvokeSchoolAdmin(String claimEmail)
        {
            SchoolAdmin? schoolAdmin = _appDbContext
                .SchoolAdmins.Where(_schoolAdmin => _schoolAdmin.EmailAddress == claimEmail)
                .FirstOrDefault();
            ViewBag.User = schoolAdmin;
            return View();
        }
    }
}
