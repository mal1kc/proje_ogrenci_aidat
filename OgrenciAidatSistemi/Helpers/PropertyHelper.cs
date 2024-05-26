namespace OgrenciAidatSistemi.Helpers
{
    public static class PropertyHelper
    {
        public static object? GetNestedPropertyValue(object item, string field)
        {
            var nestedProps = field.Split('.');
            var value = item.GetType().GetProperty(nestedProps[0])?.GetValue(item);
            if (nestedProps.Length > 1 && value != null)
            {
                return GetNestedPropertyValue(value, string.Join(".", nestedProps.Skip(1)));
            }
            return value;
        }
    }
}
