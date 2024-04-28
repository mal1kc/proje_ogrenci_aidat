using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class Grade : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string Name { get; set; }
        public required School School { get; set; }
        public int GradeLevel { get; set; }
        public ISet<Student>? Students { get; set; }
        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                GradeSearchConfig.AllowedFieldsForSearch,
                GradeSearchConfig.AllowedFieldsForSort
            );

        public GradeView ToView(bool ignoreBidirectNav = false)
        {
            if (ignoreBidirectNav)
                return new GradeView(
                    this.Id,
                    this.Name,
                    this.GradeLevel,
                    null,
                    this.CreatedAt,
                    this.UpdatedAt
                );
            return new GradeView(
                this.Id,
                this.Name,
                this.GradeLevel,
                this.Students?.Select(s => s.ToView(ignoreBidirectNav: true)).ToHashSet(),
                this.CreatedAt,
                this.UpdatedAt
            );
        }
    }

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

    public static class GradeSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "id",
            "Name",
            "GradeLevel"
        };
        public static readonly string[] AllowedFieldsForSort = new string[]
        {
            "id",
            "Name",
            "GradeLevel"
        };
    }
}
