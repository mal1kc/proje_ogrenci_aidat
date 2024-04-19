using System.Linq.Expressions;
using System.Reflection;
// import for ViewDataDictionary
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
        where T : ISearchableModel
    {
        private readonly IQueryable<T> _sourceQueryable;
        private readonly ModelSearchConfig _searchConfig;
        private IQueryable<T> _resultedQueryable;

        public QueryableModelHelper(IQueryable<T> source, ModelSearchConfig searchConfig)
        {
            _sourceQueryable = source;
            _searchConfig = searchConfig;
            _resultedQueryable = _sourceQueryable;
        }

        public IQueryable<T> Search(string searchString, string? searchField = null)
        {
            if (string.IsNullOrEmpty(searchString))
                return _sourceQueryable;

            var resultQueryable = _sourceQueryable;

            if (string.IsNullOrEmpty(searchField))
            {
                searchString = searchString.ToLower(); // Convert search string to lowercase
                var parameter = Expression.Parameter(typeof(T), "x");
                var predicate = GetCombinedContainsExpression(parameter, searchString);

                if (predicate != null)
                {
                    resultQueryable = resultQueryable.Where(predicate);
                }
            }
            else
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                resultQueryable = resultQueryable.Where(
                    FieldContainsExpression(parameter, searchField, searchString)
                );
            }

            return resultQueryable;
        }

        private Expression<Func<T, bool>> GetCombinedContainsExpression(
            ParameterExpression parameter,
            string searchString
        )
        {
            var properties = typeof(T).GetProperties();
            Expression combinedExpression = null;

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var containsExpression = FieldContainsExpression(
                        parameter,
                        property.Name,
                        searchString
                    );

                    combinedExpression = combinedExpression == null
                        ? containsExpression.Body
                        : Expression.OrElse(combinedExpression, containsExpression.Body);
                }
            }

            return combinedExpression == null
                ? Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter)
                : Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }

        private Expression<Func<T, bool>> FieldContainsExpression(
            ParameterExpression parameter,
            string fieldName,
            string searchString
        )
        {
            MemberExpression property = Expression.Property(parameter, fieldName);
            ConstantExpression search = Expression.Constant(searchString);
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodCallExpression containsExpression = Expression.Call(property, containsMethod, search);
            return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        }


        public IQueryable<T> Sort(string fieldName, SortOrderEnum sortOrder)
        {
            if (string.IsNullOrEmpty(fieldName) || !_searchConfig.AllowedFieldsForSort.Contains(fieldName))
                return _resultedQueryable;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, fieldName);

            // Convert property type to object
            var conversion = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);

            _resultedQueryable = sortOrder == SortOrderEnum.DESC
                ? _resultedQueryable.OrderByDescending(lambda)
                : _resultedQueryable.OrderBy(lambda);

            return _resultedQueryable;
        }


        public IQueryable<T> Sort(string sortOrderStr)
        {
            if (string.IsNullOrEmpty(sortOrderStr))
                return _resultedQueryable;

            var sortOrder = SortOrderEnum.ASC;
            var fieldName = sortOrderStr;

            if (sortOrderStr.EndsWith("_desc"))
            {
                sortOrder = SortOrderEnum.DESC;
                fieldName = fieldName.Substring(0, fieldName.Length - "_desc".Length);
            }

            return Sort(fieldName, sortOrder);
        }

        public IQueryable<T> SearchAndSort(
            string searchString,
            string searchField,
            string sortOrder
        )
        {
            _resultedQueryable = Search(searchString, searchField);
            return Sort(sortOrder);
        }

        public IQueryable<T> GetResult() => _resultedQueryable;

        public IQueryable<T> GetSource() => _sourceQueryable;

        public IQueryable<T> List(
            ViewDataDictionary ViewData,
            string searchString = null,
            string searchField = null,
            string sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            ViewData["IdSortParam"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearchString"] = searchString;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            _resultedQueryable = SearchAndSort(searchString, searchField, sortOrder);
            var paginatedModel = _resultedQueryable
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            var countOfmodelopResult = _resultedQueryable.Count();
            ViewData["CurrentPageIndex"] = pageIndex;
            ViewData["TotalPages"] = (int)Math.Ceiling(countOfmodelopResult / (double)pageSize);
            ViewData["TotalItems"] = countOfmodelopResult;
            ViewData["PageSize"] = pageSize;

            return paginatedModel;
        }
    }
}
