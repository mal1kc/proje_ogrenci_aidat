using System.Data;
using System.IO.Compression;
using System.Reflection;
using ClosedXML.Excel;
using OgrenciAidatSistemi.Helpers;

namespace OgrenciAidatSistemi.Services
{
    public class ExportService(ILogger<ExportService> logger)
    {
        private readonly ILogger<ExportService> _logger = logger;

        public static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var fields = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToArray();
            return ToDataTable(items, fields);
        }

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

        public MemoryStream ExportToExcel(DataTable data, string sheetName = "Export")
        {
            using var workbook = new XLWorkbook();
            workbook.Worksheets.Add(data, sheetName);
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public MemoryStream ExportMultipleToExcel(Dictionary<string, DataTable> dataTables)
        {
            using var workbook = new XLWorkbook();
            foreach (var entry in dataTables)
            {
                workbook.Worksheets.Add(entry.Value, entry.Key);
            }
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public MemoryStream CreateZipWithExcelFiles(Dictionary<string, DataTable> dataTables)
        {
            var resultStream = new MemoryStream();
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var entry in dataTables)
                    {
                        using var entryStream = archive.CreateEntry($"{entry.Key}.xlsx").Open();
                        var excelStream = ExportToExcel(entry.Value, entry.Key);
                        excelStream.CopyTo(entryStream);
                    }
                }
                memoryStream.Position = 0;
                memoryStream.CopyTo(resultStream);
            }
            resultStream.Position = 0;
            return resultStream;
        }
    }
}
