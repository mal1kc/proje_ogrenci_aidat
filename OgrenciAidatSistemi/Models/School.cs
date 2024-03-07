
namespace OgrenciAidatSistemi.Models
{

    public class School : IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        public ISet<Student> Students { get; set; }
    }

    public class SchoolView : IBaseDbModelView
    {
        public int Id { get ; set ; }
        public DateTime createdAt { get ; set ; }
        public DateTime updatedAt { get ; set ; }
    }
}
