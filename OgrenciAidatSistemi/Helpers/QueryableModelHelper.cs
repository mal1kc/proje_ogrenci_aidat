using System.Linq;
using System.Linq.Expressions;
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
        private readonly ILogger<QueryableModelHelper<T>> _logger;
        private readonly IQueryable<T> _sourceQueryable;
        private readonly ModelSearchConfig<T> _searchConfig;
        private IQueryable<T> _resultedQueryable;

        public QueryableModelHelper(IQueryable<T> source, ModelSearchConfig<T> searchConfig)
        {
            _sourceQueryable = source;
            _searchConfig = searchConfig;
            _resultedQueryable = _sourceQueryable;
            // TODO: Add logger factory or turn this helper into a service and inject the logger
            _logger = new Logger<QueryableModelHelper<T>>(new LoggerFactory());
        }

        public IQueryable<T> Search(string? searchString, string? searchField = null)
        {
            if (string.IsNullOrEmpty(searchString))
                return _sourceQueryable;

            var resultQueryable = _sourceQueryable;

            var resultParallel = resultQueryable.AsParallel();
            if (string.IsNullOrEmpty(searchField))
            {
                // if searchField is not specified, search in all fields
                foreach (var searchMethod in _searchConfig.SearchMethods.Values)
                {
                    resultParallel = resultParallel.Where(model =>
                        searchMethod(model, searchString)
                    );
                }
            }
            else if (
                _searchConfig.SearchMethods.TryGetValue(
                    searchField,
                    out Func<T, string, bool>? value
                )
            )
            {
                // if searchField is specified, search in the specified field
                var searchMethod = value;
                resultParallel = resultParallel.Where(model => searchMethod(model, searchString));
            }
            resultQueryable = resultParallel.AsQueryable();

            return resultQueryable;
        }

        public IQueryable<T> Sort(string fieldName, SortOrderEnum sortOrder)
        {
            // if fieldName is not specified, return the current queryable
            // || if fieldName is not in the sorting methods, return the current queryable
            if (
                string.IsNullOrEmpty(fieldName)
                || !_searchConfig.SortingMethods.TryGetValue(
                    fieldName,
                    out Expression<Func<T, object>>? value
                )
            )
                return _resultedQueryable;

            var sortMethod = value.Compile();

            _resultedQueryable =
                sortOrder == SortOrderEnum.DESC
                    ? _resultedQueryable.OrderByDescending(sortMethod).AsQueryable()
                    : _resultedQueryable.OrderBy(sortMethod).AsQueryable();

            return _resultedQueryable;
        }

        public IQueryable<T> Sort(string? sortOrderStr)
        {
            if (string.IsNullOrEmpty(sortOrderStr))
                return _resultedQueryable;

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
            _resultedQueryable ??= _sourceQueryable;
            if (!string.IsNullOrEmpty(searchString))
                _resultedQueryable = Search(searchString, searchField);
            if (!string.IsNullOrEmpty(sortField))
                _resultedQueryable = Sort(sortField, sortType);
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
            // validate sortOrder check '_' is exist and split it check field and type is valid
            string? sortField = null;
            string? sortType = null;
            if (!string.IsNullOrEmpty(sortOrder) && sortOrder.Contains("_"))
            {
                var parts = sortOrder.Split('_');
                if (parts.Length == 2)
                {
                    sortField = parts[0];
                    sortType = parts[1];
                }
            }

            if (searchField != null && !_searchConfig.AllowedFieldsForSearch.Contains(searchField))
            {
                // if searchField is not in the allowed fields, reset it
                searchField = null;
                searchString = null;
            }

            if (sortType != null && sortType != "asc" && sortType != "desc")
            {
                // if sortType is not valid, reset it
                sortOrder = null;
                sortField = null;
                sortType = null;
            }

            // generate ViewData {Field}SortParam for view
            foreach (var field in _searchConfig.AllowedFieldsForSort)
            {
                // example result: FirstNameSortParam = FirstName_desc or FirstName_asc

                ViewData[$"{field}SortParam"] =
                    sortOrder == null
                        ? $"{field}_asc"
                        : field == sortField
                            ? sortType == "asc"
                                ? $"{field}_desc"
                                : $"{field}_asc"
                            : $"{field}_asc";

                if (field == sortField)
                {
                    ViewData["CurrentSortField"] = field;
                    ViewData["CurrentSortType"] = sortType;
                    ViewData["CurrentSortOrder"] = sortOrder;
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
            var paginatedModel = _resultedQueryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            int countOfmodelopResult;
            // try to evaluate the query before getting the count
            countOfmodelopResult = _resultedQueryable.Count();

            // _logger.LogError(
            //     message: "Error : {} while evaluating the query for pagination details : {}",
            //     _resultedQueryable.Expression.ToString()

            ViewData["CurrentPageIndex"] = pageIndex;
            ViewData["TotalPages"] = (int)Math.Ceiling(countOfmodelopResult / (double)pageSize);
            ViewData["TotalItems"] = countOfmodelopResult;
            ViewData["PageSize"] = pageSize;

            return paginatedModel;
        }
    }
}
