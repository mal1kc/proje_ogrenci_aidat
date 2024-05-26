using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Services
{
    public class ExportService(DbContext context, ILogger<ExportService> logger)
    {
        private readonly DbContext _context = context;
        private readonly ILogger _logger = logger;

        public static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var fields = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToArray();
            return ToDataTable(items, fields);
        }

        // with specified fields(as column names)
        public static DataTable ToDataTable<T>(IEnumerable<T> items, string[] fields)
        {
            var dataTable = new DataTable(typeof(T).Name);
            foreach (var field in fields)
            {
                dataTable.Columns.Add(field);
            }
            foreach (var item in items)
            {
                if (item == null)
                    continue;
                var values = new object[fields.Length];
                for (var i = 0; i < fields.Length; i++)
                {
                    values[i] = PropertyHelper.GetNestedPropertyValue(item, fields[i]) ?? "";
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public MemoryStream ExportToExcel(DataTable data)
        {
            using var workbook = new XLWorkbook();
            workbook.Worksheets.Add(data, "Export");
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
