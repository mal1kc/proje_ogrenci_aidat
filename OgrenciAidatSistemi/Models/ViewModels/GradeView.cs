using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class GradeView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GradeLevel { get; set; }
        public ISet<StudentView>? Students { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public GradeView(
            int id,
            string name,
            int gradeLevel,
            ISet<StudentView>? students,
            DateTime createdAt,
            DateTime updatedAt
        )
        {
            Id = id;
            Name = name;
            GradeLevel = gradeLevel;
            Students = students;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
