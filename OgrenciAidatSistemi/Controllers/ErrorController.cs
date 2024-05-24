using Microsoft.AspNetCore.Mvc;

namespace OgrenciAidatSistemi.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int? statusCode = null)
        {
            ViewBag.Title = "Error";
            switch (statusCode)
            {
                case 404:
                    ViewBag.Title = "Error 404 - page not Found";
                    ViewBag.Message =
                        "The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.";
                    break;
                case 403:
                    ViewBag.Title = "Error 403 - Access Denied";
                    ViewBag.Message = "You don't have permission to access this resource.";
                    break;
                case 401:
                    ViewBag.Title = "Error 401 - Unauthorized";
                    ViewBag.Message = "You are not authorized to view this page.";
                    break;
                case 500:
                    ViewBag.Title = "Error 500 - Internal Server Error";
                    ViewBag.Message =
                        "The server encountered an internal error or misconfiguration and was unable to complete your request.";
                    break;
                default:
                    ViewBag.Title = "Error - An error occurred";
                    ViewBag.Message = "An error occurred while processing your request.";
                    break;
            }

            return View("Index");
        }
    }
}
