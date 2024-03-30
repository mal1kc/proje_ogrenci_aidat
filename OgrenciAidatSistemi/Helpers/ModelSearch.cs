using System.Linq.Expressions;
using System.Reflection;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Helpers
{
    public class ModelSearchQueryable<T>
        where T : ISearchableModel
    {
        private readonly IQueryable<T> _sourceQueryable;
        private readonly ModelSearchConfig _searchConfig;

        public ModelSearchQueryable(IQueryable<T> source, ModelSearchConfig searchConfig)
        {
            _sourceQueryable = source;
            _searchConfig = searchConfig;
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
    }
}
