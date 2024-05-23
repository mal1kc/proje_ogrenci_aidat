using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class School : BaseDbModel, ISearchableModel<School>
    {
        public required string Name { get; set; }
        public ISet<SchoolAdmin>? SchoolAdmins { get; set; }
        public ISet<Grade>? Grades { get; set; }
        public ISet<WorkYear>? WorkYears { get; set; }

        public required ISet<Student?>? Students { get; set; }

        public static ModelSearchConfig<School> SearchConfig =>
            new(
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "Name", static s => s.Name },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt }
                },
                searchMethods: new()
                {
                    {
                        "Id",
                        static (s, searchString) =>
                            s
                                .Id.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "Name",
                        static (s, searchString) =>
                            s.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    // search by year and month not complete date
                    {
                        "CreatedAt",
                        static (s, searchString) =>
                            s
                                .CreatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "UpdatedAt",
                        static (s, searchString) =>
                            s
                                .UpdatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    }
                }
            );

        public SchoolView ToView(bool ignoreBidirectNav = false)
        {
            return new SchoolView()
            {
                Id = this.Id,
                Name = this.Name,
                SchoolAdmins = ignoreBidirectNav
                    ? null
                    : this
                        .SchoolAdmins?.Select(sa => sa.ToView(ignoreBidirectNav: true))
                        .ToHashSet(),
                Students = ignoreBidirectNav
                    ? null
                    : this.Students?.Select(s => s.ToView(ignoreBidirectNav: true)).ToHashSet(),
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

        public ISet<StudentView>? Students { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public School ToModel(bool ignoreBidirectNav = false)
        {
            // TODO: Implement ToModel to all view models for more manageable code
            throw new NotImplementedException();
        }
    }
}
