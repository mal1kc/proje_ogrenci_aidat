using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Services
{
    public class ExportService(AppDbContext context, ILogger<ExportService> logger)
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;

        public static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                dataTable.Columns.Add(prop.Name);
            }
            foreach (var item in items)
            {
                var values = new object[props.Length];

                for (var i = 0; i < props.Length; i++)
                {
                    var value = props[i].GetValue(item, null);
                    if (value is DateTime time)
                    {
                        values[i] = time.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        values[i] = value ?? "";
                    }
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        // with specified fields(as column names)
        public static DataTable ToDataTable<T>(IEnumerable<T> items, string[] fields)
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
