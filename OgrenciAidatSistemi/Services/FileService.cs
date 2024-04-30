using System.Security.Cryptography;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public FileService(
            ILogger<FileService> logger,
            AppDbContext dbContext,
            IWebHostEnvironment environment
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, User createdBy)
        {
            if (
                file == null
                || file.Length == 0
                || file.Length > Configurations.Constants.MaxFileSize
            )
            {
                throw new ArgumentException("File is empty or exceeds the maximum allowed size.");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileModel = new FilePath(
                path: filePath,
                name: uniqueFileName,
                extension: Path.GetExtension(file.FileName),
                contentType: file.ContentType,
                size: file.Length,
                description: "Uploaded file"
            );

            fileModel.CreatedBy = createdBy;

            try
            {
                fileModel.FileHash = ComputeHash(filePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while calculating hash");
                throw new InvalidOperationException("Error while calculating hash");
            }

            if (_dbContext.FilePaths == null)
            {
                _logger.LogWarning("No file paths found in the database");
                _dbContext.FilePaths = _dbContext.Set<FilePath>();
                if (_dbContext.FilePaths == null)
                {
                    throw new InvalidOperationException("No file paths found in the database");
                }
            }
            try
            {
                _dbContext.FilePaths.Add(fileModel);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while saving file path to the database");
                throw new InvalidOperationException("Error while saving file path to the database");
            }
            return filePath;
        }

        public async Task<byte[]> DownloadFileAsync(int fileId)
        {
            if (fileId <= 0)
            {
                throw new ArgumentException("Invalid file id");
            }

            if (_dbContext.FilePaths == null)
            {
                _logger.LogWarning("No file paths found in the database");
                _dbContext.FilePaths = _dbContext.Set<FilePath>();
                if (_dbContext.FilePaths == null)
                {
                    throw new InvalidOperationException("No file paths found in the database");
                }
            }

            var fileModel = await _dbContext.FilePaths.FindAsync(fileId);
            if (fileModel == null)
            {
                throw new ArgumentException("File not found");
            }

            using (var stream = new FileStream(fileModel.Path, FileMode.Open))
            {
                var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                memory.Position = 0;
                return memory.ToArray();
            }
        }

        private string ComputeHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
