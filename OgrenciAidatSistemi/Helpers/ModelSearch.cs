using System.Linq.Expressions;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Helpers
{
    public class ModelSearch<T>
        where T : ISearchableModel
    {
        private readonly IQueryable<T> _sourceQueryable;
        private readonly IEnumerable<T> _sourceEnumerable;
        private readonly ModelSearchConfig _searchConfig;

        public ModelSearch(IEnumerable<T> source, ModelSearchConfig searchConfig)
        {
            _sourceEnumerable = source;
            _searchConfig = searchConfig;
        }

        public ModelSearch(IQueryable<T> source, ModelSearchConfig searchConfig)
        {
            _sourceQueryable = source;
            _searchConfig = searchConfig;
        }

        public IEnumerable<T> Search(string searchString, string? searchField = null)
        {
            return _sourceQueryable != null
                ? SearchQueryable(searchString, searchField)
                : SearchEnumerable(searchString, searchField);
        }

        private IEnumerable<T> SearchQueryable(string searchString, string? searchField)
        {
            if (string.IsNullOrEmpty(searchString))
                return _sourceQueryable;

            Expression<Func<T, bool>> predicate;
            Expression<Func<T, bool>> containsExpression;
            Expression<Func<T, string>> propertyExpression;
            // if searchField is not provided, search all allowed fields

            if (string.IsNullOrEmpty(searchField))
            {
                var result = new List<T>();
                foreach (var field in _searchConfig.AllowedFieldsForSearch)
                {
                    if (typeof(T).GetProperty(field) == null)
                        throw new ArgumentException($"Field '{field}' does not exist in model.");
                    propertyExpression = BuildPropertyExpression(field);
                    containsExpression = BuildContainsExpression(propertyExpression, searchString);
                    predicate = containsExpression;

                    result.AddRange(_sourceQueryable.Where(predicate));
                }
                // if has id field, distinct by id
                if (typeof(T).GetProperty("Id") != null)
                {
                    return result.DistinctBy(x => ((IBaseDbModel)x).Id);
                }
                return result.Distinct();
            }

            if (typeof(T).GetProperty(searchField) == null)
                throw new ArgumentException($"Field '{searchField}' does not exist in model.");

            propertyExpression = BuildPropertyExpression(searchField);
            containsExpression = BuildContainsExpression(propertyExpression, searchString);
            predicate = containsExpression;

            return _sourceQueryable.Where(predicate);
        }

        private IEnumerable<T> SearchEnumerable(string searchString, string? searchField)
        {
            if (string.IsNullOrEmpty(searchString))
                return _sourceEnumerable;

            if (string.IsNullOrEmpty(searchField))
            {
                return _sourceEnumerable.Where(item =>
                    _searchConfig.AllowedFieldsForSearch.Any(field =>
                        FieldContains(item, field, searchString)
                    )
                );
            }
            else
            {
                if (typeof(T).GetProperty(searchField) == null)
                    throw new ArgumentException($"Field '{searchField}' does not exist in model.");

                return _sourceEnumerable.Where(item =>
                    FieldContains(item, searchField, searchString)
                );
            }
        }

        private bool FieldContains(T item, string fieldName, string searchString)
        {
            var property = typeof(T).GetProperty(fieldName);
            if (property == null)
                throw new ArgumentException($"Field '{fieldName}' does not exist in model.");

            var value = property.GetValue(item);
            if (value == null)
                return false;

            return value.ToString().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private Expression<Func<T, string>> BuildPropertyExpression(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            return Expression.Lambda<Func<T, string>>(property, parameter);
        }

        private Expression<Func<T, bool>> BuildContainsExpression(
            Expression<Func<T, string>> propertyExpression,
            string searchString
        )
        {
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var value = Expression.Constant(searchString, typeof(string));
            var containsExpression = Expression.Call(propertyExpression.Body, method, value);
            return Expression.Lambda<Func<T, bool>>(
                containsExpression,
                propertyExpression.Parameters
            );
        }
    }
}
