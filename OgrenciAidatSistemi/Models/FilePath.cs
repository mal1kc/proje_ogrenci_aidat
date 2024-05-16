using System.Security.Cryptography;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class FilePath : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }

        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string Description { get; set; }

        public User CreatedBy { get; set; }
        public string FileHash { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Payment Payment { get; set; }
        public int? PaymentId { get; set; }

        public FilePath(
            string path,
            string name,
            string extension,
            string contentType,
            long size,
            string description
        )
        {
            Path = path;
            Name = name;
            Extension = extension;
            ContentType = contentType;
            Size = size;
            Description = description;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public static ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                new string[] { "Name", "Description", "Extension", "ContentType" },
                new string[] { "Name", "Description", "Extension", "ContentType" }
            );

        // get safely data of file asynchrously
        public async Task<byte[]> GetDataAsync()
        {
            byte[] data = { }; // empty array of byte
            using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                data = new byte[fs.Length];
                await fs.ReadAsync(data, 0, (int)fs.Length);
            }
            return data;
        }

        public static string ComputeHash(string filePath)
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

        public string ComputeHash()
        {
            return ComputeHash(Path);
        }
    }
}
