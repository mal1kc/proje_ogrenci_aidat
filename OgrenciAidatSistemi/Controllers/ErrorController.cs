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

            return statusCode switch
            {
                "404" => RedirectToAction("Error404"),
                "403" => RedirectToAction("Error403"),
                "401" => RedirectToAction("Error401"),
                "500" => RedirectToAction("Error500"),
                _ => View(),
            };
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

        // public PartialViewResult ErrorPartial()
        // {
        //     return PartialView("_ErrorPartial");
        // }
    }
}
