using System.Security.Cryptography;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IWebHostEnvironment _environment;

        private readonly IConfiguration _configuration;

        private long _maxFileSize;

        private string _uploadsFolder;

        public FileService(
            ILogger<FileService> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _environment = environment;
            _configuration = configuration;

            _maxFileSize = Configurations.Constants.MaxFileSize;
            _uploadsFolder = Path.Combine(
                _configuration?["UploadsFolder"] ?? _environment.WebRootPath,
                "uploads"
            );

            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }

            var maxUploadFileSizeValue = _configuration?["MaxUploadFileSize"];
            if (
                maxUploadFileSizeValue != null
                && long.TryParse(maxUploadFileSizeValue, out var parsedSize)
            )
            {
                _maxFileSize = parsedSize;
            }
        }

        public async Task<FilePath> UploadFileAsync(
            IFormFile file,
            User createdBy,
            IEnumerable<string>? allowedExtensions = null
        )
        {
            if (allowedExtensions != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException("File extension is not allowed.");
                }
            }

            if (file == null || file.Length == 0 || file.Length > _maxFileSize)
            {
                throw new ArgumentException("File is empty or exceeds the maximum allowed size.");
            }

            string uniqueFileName;
            string? filePath;
            do
            {
                uniqueFileName = Guid.NewGuid().ToString() + "_" + SanitizeFileName(file.FileName);
                filePath = Path.Combine(_uploadsFolder, uniqueFileName);
            } while (File.Exists(filePath));

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var fileHash = ComputeHash(stream);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);
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
                FileHash = fileHash,
                CreatedBy = createdBy
            };

            return filepath;
        }

        public async Task<byte[]> DownloadFileAsync(FilePath file)
        {
            if (file != null)
            {
                if (!File.Exists(file.Path))
                {
                    throw new FileNotFoundException("File not found", file.Path);
                }

                return await file.GetDataAsync();
            }

            throw new ArgumentNullException(nameof(file));
        }

        public async Task<MemoryStream> DownloadFileAsStreamAsync(FilePath file)
        {
            if (file != null)
            {
                if (!File.Exists(file.Path))
                {
                    throw new FileNotFoundException("File not found", file.Path);
                }

                return await file.GetDataAsStreamAsync();
            }

            throw new ArgumentNullException(nameof(file));
        }

        private static string ComputeHash(MemoryStream memoryStream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(memoryStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public void DeleteFile(string fileName)
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var filePath = Path.Combine(_uploadsFolder, sanitizedFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                throw new FileNotFoundException($"The file {fileName} does not exist.");
            }
        }

        public async Task<FilePath> WriteFileAsync(
            string fileName,
            string content,
            User createdBy,
            string folderPath = "/generated"
        )
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty.");
            }
            var sanitizedFileName = SanitizeFileName(fileName);
            var filePath = Path.Combine(_uploadsFolder, folderPath, sanitizedFileName);
            await File.WriteAllTextAsync(filePath, content);
            return new FilePath(
                path: filePath,
                name: fileName,
                extension: Path.GetExtension(fileName),
                contentType: "text/plain",
                size: content.Length,
                description: "Generated file"
            );
        }

        public static string SanitizeFileName(string fileName)
        {
            // Remove invalid characters from the file name
            string invalidChars =
                new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            // Remove certain patterns from the file name
            string[] patterns = { "/.", "..", ".", "~", "/", "\\" };
            foreach (string pattern in patterns)
            {
                fileName = fileName.Replace(pattern, "");
            }

            return fileName;
        }
    }
}
