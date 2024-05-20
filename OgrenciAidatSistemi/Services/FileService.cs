using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<FilePath> UploadFileAsync(IFormFile file, User createdBy)
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

            var filepath = new FilePath(
                path: filePath,
                name: uniqueFileName,
                extension: Path.GetExtension(file.FileName),
                contentType: file.ContentType,
                size: file.Length,
                description: "Uploaded file"
            )
            {
                CreatedBy = createdBy
            };

            try
            {
                filepath.FileHash = ComputeHash(filePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while calculating hash");
                throw new InvalidOperationException("Error while calculating hash");
            }
            return filepath;
        }

        public async Task<byte[]> DownloadFileAsync(FilePath file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!File.Exists(file.Path))
            {
                throw new FileNotFoundException("File not found", file.Path);
            }

            return await file.GetDataAsync();
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
