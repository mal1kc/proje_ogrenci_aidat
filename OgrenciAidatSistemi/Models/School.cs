using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class School : IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; }
        public ISet<SchoolAdmin>? SchoolAdmins { get; set; }

        public ISet<Student> Students { get; set; }
    }

    public class SchoolView : IBaseDbModelView
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
