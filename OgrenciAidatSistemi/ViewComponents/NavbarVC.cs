using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

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
                    _ => InvokeSiteAdmin(claimEmail)
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
    }
}
