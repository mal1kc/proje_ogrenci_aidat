using Microsoft.AspNetCore.Mvc;

namespace OgrenciAidatSistemi.Controllers
{
    public class BaseModelController(ILogger<BaseModelController> logger) : Controller
    {
        private readonly ILogger<BaseModelController> _logger = logger;

        [NonAction]
        public IActionResult TryListOrFail<T>(
            Func<T> listFunc,
            string modelPluralName = "school admins"
        ) // "school admins
        {
            try
            {
                return View(listFunc());
            }
            catch (Exception e)
            {
                TempData["Error"] =
                    $"Error while listing {modelPluralName} If the problem persists, please contact the administrator.";
                _logger.LogError(e, $"Error while listing {modelPluralName}");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
