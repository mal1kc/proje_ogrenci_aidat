using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    public class School : BaseDbModel, ISearchableModel<School>
    {
        public required string Name { get; set; }
        public ICollection<SchoolAdmin>? SchoolAdmins { get; set; }
        public ICollection<Grade>? Grades { get; set; }
        public ICollection<WorkYear>? WorkYears { get; set; }

        public required ISet<Student?>? Students { get; set; }

        public static ModelSearchConfig<School> SearchConfig =>
            new(
                defaultSortMethod: s => s.CreatedAt,
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
                    : this.Students?.Select(s => s.ToView(ignoreBidirectNav: true)).ToList(),
                WorkYears = ignoreBidirectNav
                    ? null
                    : this.WorkYears?.Select(wy => wy.ToView(ignoreBidirectNav: true)).ToList(),
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }
    }
}
