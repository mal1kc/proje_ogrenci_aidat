using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Tests
{
    public class FileServiceTests
    {
        private readonly FileService _fileService;
        private readonly Mock<IFormFile> _mockFile;
        private readonly Mock<User> _mockUser;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public FileServiceTests()
        {
            var mockLogger = new Mock<ILogger<FileService>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(m => m.WebRootPath).Returns("testpath");

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.SetupGet(m => m["UploadsFolder"]).Returns("testpath");
            _mockConfiguration.SetupGet(m => m["MaxUploadFileSize"]).Returns("1048576");

            Assert.NotNull(_mockEnvironment.Object.WebRootPath);
            Assert.NotNull(_mockConfiguration.Object["UploadsFolder"]);
            Assert.NotNull(_mockConfiguration.Object["MaxUploadFileSize"]);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // Unique name for in-memory database
                .Options;

            _fileService = new FileService(
                mockLogger.Object,
                _mockEnvironment.Object,
                _mockConfiguration.Object
            );

            _mockFile = new Mock<IFormFile>();
            _mockUser = new Mock<User>();
        }

        // ... rest of your tests

        [Fact]
        public async Task UploadFileAsync_ShouldReturnFilePath_WhenFileIsValid()
        {
            // Arrange
            var memoryStream = new MemoryStream();
            _mockFile
                .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);
            _mockFile.Setup(f => f.Length).Returns(1);
            _mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act
            var result = await _fileService.UploadFileAsync(_mockFile.Object, _mockUser.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FilePath>(result);
        }

        [Fact]
        public async Task DownloadFileAsync_ShouldReturnFile_WhenFileExists()
        {
            // Arrange
            var fileName = "test.txt";
            var filePath = Path.Combine(_mockEnvironment.Object.WebRootPath, "uploads", fileName);
            using (var stream = File.Create(filePath))
            {
                var writer = new StreamWriter(stream);
                await writer.WriteAsync("Test content");
                await writer.FlushAsync();
            }

            FilePath file =
                new(
                    path: filePath,
                    name: fileName,
                    extension: ".txt",
                    contentType: "text/plain",
                    size: 1,
                    description: "Test file"
                );

            // Act
            var result = await _fileService.DownloadFileAsync(file);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<byte[]>(result);
        }

        [Fact]
        public async Task DowloadFileStreamAsync_ShouldReturnFile_WhenFileExists()
        {
            // Arrange
            var fileName = "test.txt";
            var filePath = Path.Combine(_mockEnvironment.Object.WebRootPath, "uploads", fileName);
            using (var stream = File.Create(filePath))
            {
                var writer = new StreamWriter(stream);
                await writer.WriteAsync("Test content");
                await writer.FlushAsync();
            }

            FilePath file =
                new(
                    path: filePath,
                    name: fileName,
                    extension: ".txt",
                    contentType: "text/plain",
                    size: 1,
                    description: "Test file"
                );

            // Act
            var result = await _fileService.DownloadFileAsStreamAsync(file);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MemoryStream>(result);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldDeleteFile_WhenFileExists()
        {
            // Arrange
            var fileName = "test.txt";
            var filePath = Path.Combine(_fileService._uploadsFolder, fileName);

            using (var stream = File.Create(filePath))
            {
                var writer = new StreamWriter(stream);
                await writer.WriteAsync("Test content");
                await writer.FlushAsync();
            }

            // Act
            _fileService.DeleteFile(fileName);

            // Assert
            Assert.False(File.Exists(filePath));
        }

        // check file size restrictions
        [Fact]
        public async Task UploadFileAsync_ShouldThrowException_WhenFileIsTooLarge()
        {
            // Arrange
            _mockFile.Setup(f => f.Length).Returns(1048577);
            _mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act
            async Task Act() =>
                await _fileService.UploadFileAsync(_mockFile.Object, _mockUser.Object);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Act);
        }
    }
}
