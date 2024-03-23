namespace OgrenciAidatSistemi.Models
{
    public class FilePath : IBaseDbModel
    {
        public int Id { get; set; }

        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        FilePath()
        {
            throw new System.NotImplementedException();
        }
    }
}
