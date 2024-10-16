using System.Security.Cryptography;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class FilePath(
        string path,
        string name,
        string extension,
        string contentType,
        long size,
        string description
    ) : BaseDbModel
    {
        public string? Path { get; set; } = path;
        public string Name { get; set; } = name;
        public string Extension { get; set; } = extension;
        public string ContentType { get; set; } = contentType;
        public long Size { get; set; } = size;
        public string Description { get; set; } = description;

        public User CreatedBy { get; set; }
        public string FileHash { get; set; }

        // get safely data of file asynchrously
        public async Task<byte[]> GetDataAsync()
        {
            if (!File.Exists(Path))
                throw new FileNotFoundException("File not found", Path);
            byte[] data = []; // empty array of byte
            using (FileStream fs = new(Path, FileMode.Open, FileAccess.Read))
            {
                data = new byte[fs.Length];
                await fs.ReadAsync(data.AsMemory(0, (int)fs.Length));
            }
            return data;
        }

        public static string ComputeHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public string ComputeHash()
        {
            if (Path == null)
                throw new ArgumentNullException(nameof(Path));
            return ComputeHash(Path);
        }

        internal async Task<MemoryStream> GetDataAsStreamAsync()
        {
            var memoryStream = new MemoryStream();
            if (!File.Exists(Path))
                throw new FileNotFoundException("File not found", Path);
            using (var fileStream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
