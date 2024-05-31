using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Helpers
{
    public enum SortOrderEnum
    {
        ASC,
        DESC
    }

    public class QueryableModelHelper<T>
        where T : ISearchableModel<T>
    {
        private readonly IQueryable<T> _sourceQueryable;
        private readonly ModelSearchConfig<T> _searchConfig;
        private IQueryable<T> _resultedQueryable;

        public QueryableModelHelper(IQueryable<T> source, ModelSearchConfig<T> searchConfig)
        {
            _sourceQueryable = source;
            _searchConfig = searchConfig;
            _resultedQueryable = _sourceQueryable;
        }

        public IQueryable<T> Search(string? searchString, string? searchField = null)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length < 3)
                return _sourceQueryable;

            var query = _sourceQueryable.AsQueryable();

            if (string.IsNullOrEmpty(searchField))
            {
                query = query
                    .AsParallel()
                    .Where(model =>
                        _searchConfig.SearchMethods.Values.Any(searchMethod =>
                            searchMethod(model, searchString)
                        )
                    )
                    .AsQueryable();
            }
            else if (_searchConfig.SearchMethods.TryGetValue(searchField, out var searchMethod))
            {
                query = query
                    .AsParallel()
                    .Where(model => searchMethod(model, searchString))
                    .AsQueryable();
            }

            return query;
        }

        public IQueryable<T> Sort(string fieldName, SortOrderEnum sortOrder)
        {
            if (
                string.IsNullOrEmpty(fieldName)
                || !_searchConfig.SortingMethods.TryGetValue(fieldName, out var sortExpression)
            )
                return _resultedQueryable;

            _resultedQueryable =
                sortOrder == SortOrderEnum.DESC
                    ? _resultedQueryable.OrderByDescending(sortExpression)
                    : _resultedQueryable.OrderBy(sortExpression);

            return _resultedQueryable;
        }

        public IQueryable<T> Sort(string? sortOrderStr)
        {
            if (string.IsNullOrEmpty(sortOrderStr))
                return _resultedQueryable.OrderBy(_searchConfig.DefaultSortMethod);

            var sortOrder = SortOrderEnum.ASC;
            var fieldName = sortOrderStr;

            if (sortOrderStr.EndsWith("_desc"))
            {
                sortOrder = SortOrderEnum.DESC;
                fieldName = fieldName[..^"_desc".Length];
            }

            return Sort(fieldName, sortOrder);
        }

        public IQueryable<T> SearchAndSort(
            string? searchString,
            string? searchField,
            string? sortField,
            SortOrderEnum sortType
        )
        {
            _resultedQueryable = _sourceQueryable;

            if (!string.IsNullOrEmpty(searchString))
                _resultedQueryable = Search(searchString, searchField);

            if (!string.IsNullOrEmpty(sortField))
                _resultedQueryable = Sort(sortField, sortType);
            else
                _resultedQueryable = _resultedQueryable.OrderBy(_searchConfig.DefaultSortMethod);

            return _resultedQueryable;
        }

        public IQueryable<T> GetResult() => _resultedQueryable;

        public IQueryable<T> GetSource() => _sourceQueryable;

        public IQueryable<T> List(
            ViewDataDictionary ViewData,
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Clamp(pageSize, 10, 100);

            // Validate and parse sortOrder
            string? sortField = null;
            string? sortType = null;
            if (!string.IsNullOrEmpty(sortOrder))
            {
                var parts = sortOrder.Split('_');
                if (
                    parts.Length == 2
                    && _searchConfig.AllowedFieldsForSort.Contains(parts[0])
                    && (parts[1] == "asc" || parts[1] == "desc")
                )
                {
                    sortField = parts[0];
                    sortType = parts[1];
                }
            }

            // Generate ViewData for sorting
            foreach (var field in _searchConfig.AllowedFieldsForSort)
            {
                var sortOrderKey = $"{field}SortOrder";
                var sortOrderValue =
                    sortOrder == null
                        ? $"{field}_asc"
                        : field == sortField
                            ? sortType == "asc"
                                ? $"{field}_desc"
                                : $"{field}_asc"
                            : $"{field}_asc";

                ViewData[sortOrderKey] = sortOrderValue;
                if (field == sortField)
                {
                    ViewData["CurrentSortOrder"] = ViewData[sortOrderKey];
                }
            }

            ViewData["CurrentSearchString"] = searchString;
            ViewData["CurrentSearchField"] = searchField;

            _resultedQueryable = SearchAndSort(
                searchString,
                searchField,
                sortField,
                sortType == "asc" ? SortOrderEnum.ASC : SortOrderEnum.DESC
            );

            // Pagination
            var paginatedModel = _resultedQueryable
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var countOfModelResult = _resultedQueryable.Count();

            ViewData["CurrentPageIndex"] = pageIndex;
            ViewData["TotalPages"] = (int)Math.Ceiling(countOfModelResult / (double)pageSize);
            ViewData["TotalItems"] = countOfModelResult;
            ViewData["PageSize"] = pageSize;

            return paginatedModel.AsQueryable();
        }
    }
}
