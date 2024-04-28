using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class FileController : Controller
    {
        private readonly ILogger<FileController> _logger;
        private readonly AppDbContext _dbContext;
        private readonly FileService _fileService;
        private readonly UserService _userService;

        public FileController(
            ILogger<FileController> logger,
            AppDbContext dbContext,
            UserService userService,
            FileService fileService
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileService = fileService;
            _userService = userService;
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var filePath = await _fileService.UploadFileAsync(file, currentUser);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var fileContent = await _fileService.DownloadFileAsync(id);
                return File(fileContent, "application/octet-stream");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
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
    }
}
