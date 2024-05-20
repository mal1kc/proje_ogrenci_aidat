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
                _logger.LogWarning(ex, "File not found");
                return RedirectToAction("Index", "Error", new { statusCode = 404 });
            }
            catch (Exception ex)
            {
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
            ViewBag.IsSiteAdmin = false;
            _dbContext.Receipts ??= _dbContext.Set<Receipt>();
            IQueryable<Receipt>? receipts = null;

            var (role, schoolid) = _userService.GetUserRoleAndSchoolId().Result;

            if (role == null)
            {
                return Unauthorized();
            }

            switch (role)
            {
                case UserRole.SiteAdmin:
                    ViewBag.IsSiteAdmin = true;
                    receipts = _dbContext
                        .Receipts.Include(r => r.CreatedBy)
                        .Include(r => r.Payment)
                        .Include(r => r.Payment.Student)
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
                return View(
                    modelList.List(
                        ViewData,
                        searchString,
                        searchField,
                        sortOrder,
                        pageIndex,
                        pageSize
                    )
                );
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
    }
}
