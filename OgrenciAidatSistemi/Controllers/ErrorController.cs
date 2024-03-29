using Microsoft.AspNetCore.Mvc;

namespace OgrenciAidatSistemi.Controllers
{
    public class ErrorController : Controller
    {
        // "/Error?statusCode=404"

        public ActionResult Index(string? statusCode)
        {
            ViewBag.Title = "Error";

            // if (statusCode == "404")
            // {
            //     return RedirectToAction("Error404");
            // }

            switch (statusCode)
            {
                case "404":
                    return RedirectToAction("Error404");
                case "403":
                    return RedirectToAction("Error403");
                case "401":
                    return RedirectToAction("Error401");
                case "500":
                    return RedirectToAction("Error500");
                default:
                    return View();
            }
        }

        public ActionResult Error404()
        {
            ViewBag.Title = "Error 404 - page not Found";
            return View("Index");
        }

        public ActionResult Error403()
        {
            ViewBag.Title = "Error 403 - Access Denied";
            return View("Index");
        }

        public ActionResult Error401()
        {
            ViewBag.Title = "Error 401 - Unauthorized";
            return View("Index");
        }

        public ActionResult Error500()
        {
            ViewBag.Title = "Error 500 - Internal Server Error";
            return View("Index");
        }
    }
}
