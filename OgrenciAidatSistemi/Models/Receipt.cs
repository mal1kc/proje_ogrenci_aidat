using System.Security.Cryptography;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class Receipt : FilePath, ISearchableModel
    {
        public Payment? Payment { get; set; }
        public int? PaymentId { get; set; }

        public static ModelSearchConfig SearchConfig =>
            new(
                ["Name", "Description", "Extension", "ContentType", "PaymentId"],
                ["Name", "Description", "Extension", "ContentType", "PaymentId"]
            );

        public Receipt(
            string path,
            string name,
            string extension,
            string contentType,
            long size,
            string description
        )
            : base(path, name, extension, contentType, size, description)
        {
            // Add constructor body here
        }
    }
}
