using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    public class Grade : BaseDbModel, ISearchableModel<Grade>
    {
        public required string Name { get; set; }
        public School? School { get; set; }
        public int GradeLevel { get; set; }
        public ISet<Student>? Students { get; set; }
        public static ModelSearchConfig<Grade> SearchConfig =>
            new(
                defaultSortMethod: s => s.CreatedAt,
                sortingMethods: new()
                {
                    { "Name", static s => s.Name },
                    { "GradeLevel", static s => s.GradeLevel },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt }
                },
                searchMethods: new()
                {
                    {
                        "Name",
                        static (s, searchString) =>
                            s.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "GradeLevel",
                        static (s, searchString) =>
                            s
                                .GradeLevel.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
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
}
