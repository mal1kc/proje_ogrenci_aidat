using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly ILogger<ReceiptController> _logger;
        private readonly AppDbContext _dbContext;
        private readonly ReceiptService _receiptService;
        private readonly UserService _userService;

        public ReceiptController(
            ILogger<ReceiptController> logger,
            AppDbContext dbContext,
            UserService userService,
            ReceiptService receiptService
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _receiptService = receiptService;
            _userService = userService;
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, Payment payment)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var filePath = await _receiptService.CreateReceiptAsync(file, payment, currentUser);
                return Ok(new { FilePath = filePath });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while uploading the file"
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var fileContent = await _receiptService.DownloadReceiptAsync(id);
                return File(fileContent, "application/octet-stream");
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = "File not found";
                _logger.LogWarning(ex, "File not found");
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while downloading the file";
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while downloading the file"
                );
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            searchField ??= "";
            searchString ??= "";
            sortOrder ??= "";

            if (searchField.Length > 70 || searchString.Length > 70 || sortOrder.Length > 70)
            {
                return BadRequest("Search field and search string must be less than 70 characters");
            }
            ViewBag.IsSiteAdmin = false;

            IQueryable<Receipt>? receipts = null;

            var (role, schoolid) = _userService.GetUserRoleAndSchoolId().Result;

            if (role == null)
            {
                return Unauthorized();
            }
            ViewBag.UserRole = role;

            switch (role)
            {
                case UserRole.SiteAdmin:
                    ViewBag.IsSiteAdmin = true;
                    receipts = _dbContext
                        .Receipts.Include(r => r.CreatedBy)
                        .Include(r => r.Payment)
                        .Include(r => r.Payment.Student)
                        .ThenInclude(p => p.PaymentPeriods)
                        .AsQueryable();
                    break;
                case UserRole.SchoolAdmin:
                    if (schoolid == 0 || schoolid == null)
                    {
                        _logger.LogError("School id is null at Receipt/List");
                        return Unauthorized();
                    }
                    receipts = _dbContext
                        .Receipts.Where(r =>
                            r.Payment != null
                            && r.Payment.School != null
                            && r.Payment.School.Id == schoolid
                        )
                        .Include(r => r.CreatedBy)
                        .Include(r => r.Payment)
                        .Include(r => r.Payment.Student)
                        .ThenInclude(p => p.PaymentPeriods)
                        .AsQueryable();
                    break;
                case UserRole.Student:
                    if (schoolid == 0 || schoolid == null)
                    {
                        _logger.LogError("School id is null at Receipt/List");
                        return Unauthorized();
                    }
                    receipts = _dbContext
                        .Receipts.Where(r =>
                            r.Payment != null
                            && r.Payment.School != null
                            && r.Payment.School.Id == schoolid
                        )
                        .Include(r => r.CreatedBy)
                        .Include(r => r.Payment)
                        .AsQueryable();
                    break;
                default:
                    return Unauthorized();
            }

            try
            {
                // return empty Queryable
                receipts ??= _dbContext.Receipts.Where(r => false);
                var modelList = new QueryableModelHelper<Receipt>(receipts, Receipt.SearchConfig);
                var result = modelList.List(
                    ViewData,
                    searchString.SanitizeString(),
                    searchField.SanitizeString(),
                    sortOrder.SanitizeString(),
                    pageIndex,
                    pageSize
                );
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while listing files"
                );
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var receipt = await _dbContext
                    .Receipts.Include(r => r.CreatedBy)
                    .Include(r => r.Payment)
                    .Include(r => r.Payment.Student)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (receipt == null)
                {
                    return RedirectToAction("Index", "Error", new { statusCode = 404 });
                }

                var (role, schoolid) = _userService.GetUserRoleAndSchoolId().Result;

                if (role == null)
                {
                    TempData["Error"] = "You are not authorized to view this receipt";
                    return Unauthorized();
                }

                if (role == UserRole.Student && receipt.Payment?.School?.Id != schoolid)
                {
                    TempData["Error"] = "You are not authorized to view this receipt";
                    return Unauthorized();
                }
                ViewBag.UserRole = role;

                return View(receipt.ToView());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file details");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while getting file details"
                );
            }
        }
    }
}
