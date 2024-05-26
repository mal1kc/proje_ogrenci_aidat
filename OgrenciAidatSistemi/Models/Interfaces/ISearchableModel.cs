using System.Linq.Expressions;

namespace OgrenciAidatSistemi.Models.Interfaces
{
    public interface ISearchableModel<T>
    {
        public static ModelSearchConfig<T> SearchConfig { get; }
    }

    public class ModelSearchConfig<T>(
        Dictionary<string, Expression<Func<T, object>>> sortingMethods,
        Dictionary<string, Func<T, string, bool>> searchMethods
    )
    {
        public string[] AllowedFieldsForSearch { get; } = [.. searchMethods.Keys];
        public string[] AllowedFieldsForSort { get; } = [.. sortingMethods.Keys];
        public Dictionary<string, Expression<Func<T, object>>> SortingMethods { get; } =
            sortingMethods;
        public Dictionary<string, Func<T, string, bool>> SearchMethods { get; } = searchMethods;
    }
}
