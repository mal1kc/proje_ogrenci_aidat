using System.Security.Cryptography;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class Receipt(
        string path,
        string name,
        string extension,
        string contentType,
        long size,
        string description
    ) : FilePath(path, name, extension, contentType, size, description), ISearchableModel
    {
        public Payment? Payment { get; set; }
        public int? PaymentId { get; set; }

        public static ModelSearchConfig SearchConfig =>
            new(
                ["Name", "Description", "Extension", "ContentType", "PaymentId"],
                ["Name", "Description", "Extension", "ContentType", "PaymentId"]
            );

        public ReceiptView ToView()
        {
            return new()
            {
                Id = Id,
                Path = Path,
                Name = Name,
                Extension = Extension,
                ContentType = ContentType,
                Size = Size,
                Description = Description,
                CreatedBy = CreatedBy,
                FileHash = FileHash,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Payment = Payment?.ToView(ignoreBidirectNav: true),
                PaymentId = PaymentId
            };
        }
    }

    public class ReceiptView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string? Path { get; set; }
        public string? Name { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
        public long Size { get; set; }
        public string? Description { get; set; }
        public User? CreatedBy { get; set; }
        public string? FileHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PaymentView? Payment { get; set; }
        public int? PaymentId { get; set; }
    }
}
