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

            IQueryable<T> resultQueryable = _sourceQueryable;

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

                    if (combinedExpression == null)
                    {
                        combinedExpression = containsExpression.Body;
                    }
                    else
                    {
                        combinedExpression = Expression.OrElse(
                            combinedExpression,
                            containsExpression.Body
                        );
                    }
                }
            }

            if (combinedExpression == null)
            {
                // No string properties found, return a default true expression
                return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);
            }

            return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }

        // FieldContains with expression trees

        private Expression<Func<T, bool>> FieldContainsExpression(
            ParameterExpression parameter,
            string fieldName,
            string searchString
        )
        {
            MemberExpression property = Expression.Property(parameter, fieldName);
            ConstantExpression search = Expression.Constant(searchString);

            // Call Contains method to check if the property contains the search string
            MethodInfo containsMethod = typeof(string).GetMethod(
                "Contains",
                new[] { typeof(string) }
            );
            MethodCallExpression containsExpression = Expression.Call(
                property,
                containsMethod,
                search
            );

            return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        }

        public IQueryable<T> Sort(string FieldName, SortOrderEnum sortOrder)
        {
            IQueryable<T> operationQueryable;
            if (_resultedQueryable == null)
                operationQueryable = _sourceQueryable;
            else
                operationQueryable = _resultedQueryable;

            if (string.IsNullOrEmpty(FieldName))
                return operationQueryable;
            if (!_searchConfig.AllowedFieldsForSort.Contains(FieldName))
                return operationQueryable;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, FieldName);
            var lambda = Expression.Lambda<Func<T, object>>(property, parameter);
            if (sortOrder == SortOrderEnum.DESC)
                operationQueryable = operationQueryable.OrderByDescending(lambda);
            else
                operationQueryable = operationQueryable.OrderBy(lambda);
            return operationQueryable;
        }

        public IQueryable<T> Sort(string sortOrderStr)
        {
            // examples for sortOrderStr -> name_desc, name
            if (string.IsNullOrEmpty(sortOrderStr))
                return _resultedQueryable;
            var sortOrder = SortOrderEnum.ASC;
            var fieldName = sortOrderStr;

            if (sortOrderStr.Contains("_desc"))
                sortOrder = SortOrderEnum.DESC;
            fieldName = fieldName.Replace("_desc", "");
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

        public IQueryable<T> GetResult()
        {
            return _resultedQueryable;
        }

        public IQueryable<T> GetSource()
        {
            return _sourceQueryable;
        }

        public IQueryable<T> List(
        ViewDataDictionary ViewData,
            string searchString = null,
            string searchField = null,
            string sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearchString"] = searchString;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";
            _resultedQueryable = SearchAndSort(searchString, searchField, sortOrder);
            var paginatedModel = _resultedQueryable
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
                .AsQueryable();
            var countOfmodelopResult = _resultedQueryable.Count();
            ViewData["CurrentPageIndex"] = pageIndex;
            ViewData["TotalPages"] = (int)
                Math.Ceiling(countOfmodelopResult / (double)pageSize);
            ViewData["TotalItems"] = countOfmodelopResult;
            ViewData["PageSize"] = pageSize;
            return paginatedModel;
        }
    }
}
