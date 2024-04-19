using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class School : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string Name { get; set; }
        public ISet<SchoolAdmin>? SchoolAdmins { get; set; }

        public required ISet<Student> Students { get; set; }

        public ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
            SchoolSearchConfig.AllowedFieldsForSearch,
            SchoolSearchConfig.AllowedFieldsForSort
            );

        public SchoolView ToView()
        {
            return new SchoolView()
            {
                Id = this.Id,
                Name = this.Name,
                SchoolAdmins = this
                    .SchoolAdmins?.Select(schoolAdmin => schoolAdmin.ToView())
                    .ToHashSet(),
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }
    }

    public class SchoolView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ISet<SchoolAdminView>? SchoolAdmins { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public static class SchoolSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "id",
            "Name"
        };
        public static readonly string[] AllowedFieldsForSort = new string[]
        {
            "id",
            "Name"
        };
    }
}
