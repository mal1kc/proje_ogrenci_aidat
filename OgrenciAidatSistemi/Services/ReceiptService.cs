using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class ReceiptService
    {
        private readonly ILogger<ReceiptService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        private readonly FileService _fileService;

        public ReceiptService(
            ILogger<ReceiptService> logger,
            AppDbContext dbContext,
            IWebHostEnvironment environment,
            FileService fileService
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _environment = environment;
            _fileService = fileService;
        }

        public async Task<Receipt> CreateReceiptAsync(
            IFormFile file,
            Payment payment,
            User createdBy
        )
        {
            FilePath? filePath = null;
            try
            {
                filePath = await _fileService.UploadFileAsync(file, createdBy);
                if (filePath.Path == null)
                {
                    throw new ArgumentException("Error while uploading file.");
                }
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Error while uploading file.");
                throw;
            }
            var receipt = new Receipt(
                path: filePath.Path,
                name: filePath.Name,
                extension: filePath.Extension,
                contentType: filePath.ContentType,
                size: filePath.Size,
                description: "Receipt for payment"
            )
            {
                Payment = payment
            };
            if (receipt.Payment == null)
            {
                throw new ArgumentException("Payment is required for receipt.");
            }

            // if _dbContext.Receipts is null, set it to _dbContext.Set<Receipt>()
            _dbContext.Receipts ??= _dbContext.Set<Receipt>();

            try
            {
                _dbContext.Receipts.Add(receipt);
                await _dbContext.SaveChangesAsync();
                return receipt;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while saving receipt to database.");
                throw new InvalidOperationException("Error while saving receipt to database.");
            }
        }

        public async Task<Receipt?> GetReceiptByIdAsync(int id)
        {
            if (id <= 0 || id > int.MaxValue)
            {
                throw new ArgumentException("Id must be greater than 0.");
            }
            if (_dbContext.Receipts == null)
            {
                throw new InvalidOperationException("Receipts table is not found.");
            }
            return await _dbContext.Receipts.FindAsync(id);
        }

        public async Task<byte[]> DownloadReceiptAsync(int id)
        {
            var receipt =
                await GetReceiptByIdAsync(id) ?? throw new ArgumentException("Receipt not found.");
            return await _fileService.DownloadFileAsync(receipt);
        }
    }
}
