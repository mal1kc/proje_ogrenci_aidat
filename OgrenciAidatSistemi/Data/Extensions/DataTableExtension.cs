using System.Data;
using System.Reflection;

namespace OgrenciAidatSistemi.Data.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable CopyToDataTable<T>(this IEnumerable<T> items, string[] fields)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                dataTable.Columns.Add(field);
            }
            foreach (var item in items)
            {
                var values = new object[fields.Length];
                for (var i = 0; i < fields.Length; i++)
                {
                    var prop = props.FirstOrDefault(p => p.Name == fields[i]);
                    var value = prop?.GetValue(item, null);
                    values[i] = value ?? "";
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}
