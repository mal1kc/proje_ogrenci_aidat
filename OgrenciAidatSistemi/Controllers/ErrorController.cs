using Microsoft.AspNetCore.Mvc;

namespace OgrenciAidatSistemi.Controllers
{

public class ErrorController : Controller
{
    // "/Error?statusCode=404"

    public ActionResult Index(string? statusCode)
    {
        ViewBag.Title = "Error";

        if (statusCode == "404")
        {
            return RedirectToAction("Error404");
        }

        return View();
    }

    public ActionResult Error404()
    {
        ViewBag.Title = "Error 404 - page not Found";
        return View("Index");
    }
}
}
