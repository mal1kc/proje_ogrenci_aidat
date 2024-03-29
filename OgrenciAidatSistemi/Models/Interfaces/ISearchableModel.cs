namespace OgrenciAidatSistemi.Models.Interfaces
{
    public interface ISearchableModel
    {
        public ModelSearchConfig SearchConfig { get; }
    }

    public class ModelSearchConfig
    {
        public string[] AllowedFieldsForSearch { get; }
        public string[] AllowedFieldsForSort { get; }

        public ModelSearchConfig(string[] allowedFieldsForSearch, string[] allowedFieldsForSort)
        {
            AllowedFieldsForSearch = allowedFieldsForSearch;
            AllowedFieldsForSort = allowedFieldsForSort;
        }
    }
}
