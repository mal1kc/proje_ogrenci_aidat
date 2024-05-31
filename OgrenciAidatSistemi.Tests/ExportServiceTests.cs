using System.Data;
using System.IO.Compression;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OgrenciAidatSistemi.Services;
using OgrenciAidatSistemi.Tests.Data;

namespace OgrenciAidatSistemi.Tests
{
    public class ExportServiceTests
    {
        private readonly ExportService _exportService;

        public ExportServiceTests()
        {
            var mockLogger = new Mock<ILogger<ExportService>>();
            _exportService = new ExportService(mockLogger.Object);
        }

        [Fact]
        public void ToDataTable_ShouldConvertItemsToDataTable()
        {
            // Arrange
            var data = new List<TestItem>
            {
                new()
                {
                    Id = 1,
                    Name = "Item1",
                    CreatedAt = DateTime.UtcNow,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = 100.0m
                },
                new()
                {
                    Id = 2,
                    Name = "Item2",
                    CreatedAt = DateTime.UtcNow,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = 200.0m
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<TestItem>>();
            mockSet.As<IQueryable<TestItem>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<TestItem>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<TestItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet
                .As<IQueryable<TestItem>>()
                .Setup(m => m.GetEnumerator())
                .Returns(data.GetEnumerator());

            // Act
            var dataTable = ExportService.ToDataTable(data);

            // Assert
            Assert.Equal(7, dataTable.Columns.Count); // Id, Name, CreatedAt, UpdatedAt, CreatedDate, Amount
            Assert.Equal(2, dataTable.Rows.Count);
            Assert.Equal("Item1", dataTable.Rows[0]["Name"]);
            Assert.Equal(100.0m, decimal.Parse(dataTable.Rows[0]["Amount"].ToString() ?? "0"), 2); // Compare up to 2 decimal places
        }

        [Fact]
        public void ToDataTable_WithFields_ShouldConvertItemsToDataTable()
        {
            // Arrange
            var data = new List<TestItem>
            {
                new()
                {
                    Id = 1,
                    Name = "Item1",
                    CreatedAt = DateTime.UtcNow,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = 100.0m
                },
                new()
                {
                    Id = 2,
                    Name = "Item2",
                    CreatedAt = DateTime.UtcNow,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = 200.0m
                }
            }.AsQueryable();

            // _mockContext.TestItems.AddRange(data);
            // _mockContext.SaveChanges();

            // Act
            var dataTable = ExportService.ToDataTable(data, ["Name", "Amount"]);

            // Assert
            Assert.Equal(2, dataTable.Columns.Count); // Name, Amount
            Assert.Equal(2, dataTable.Rows.Count);
            Assert.Equal("Item1", dataTable.Rows[0]["Name"]);
            Assert.Equal(100.0m, decimal.Parse(dataTable.Rows[0]["Amount"].ToString() ?? "0"), 2); // Compare up to 2 decimal places
        }

        [Fact]
        public static void ToDataTable_WithRelatedData_ShouldConvertItemsToDataTable()
        {
            // Arrange
            var data = new List<TestItem>
            {
                new()
                {
                    Id = 1,
                    Name = "Item1",
                    CreatedAt = DateTime.UtcNow,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = 100.0m,
                    TestSeconds = new TestSecondItem { Name = "Related1" }
                },
                new()
                {
                    Id = 2,
                    Name = "Item2",
                    CreatedAt = DateTime.UtcNow,
                    CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = 200.0m,
                    TestSeconds = new TestSecondItem { Name = "Related2" }
                }
            }.AsQueryable();
            foreach (var item in data)
            {
                if (item.TestSeconds != null)
                {
                    item.TestSeconds.TestItems = [item];
                }
            }

            // Act
            var dataTable = ExportService.ToDataTable(
                data,
                [
                    "Name",
                    "Amount",
                    "TestSeconds.Name",
                    "CreatedAt",
                    "CreatedDate",
                    "UpdatedAt",
                    "UpdatedDate"
                ]
            );

            // Assert
            Assert.NotNull(dataTable);

            Assert.Equal(7, dataTable.Columns.Count); // Name, Amount, TestSecondItem.Name, CreatedAt, CreatedDate, UpdatedAt, UpdatedDate

            Assert.Equal(2, dataTable.Rows.Count);

            Assert.Equal("Item1", dataTable.Rows[0]["Name"]);

            Assert.Equal(200.0m, decimal.Parse(dataTable.Rows[1]["Amount"].ToString() ?? "0"), 2); // Compare up to 2 decimal places

            Assert.Equal("Related2", dataTable.Rows[1]["TestSeconds.Name"]);

            Assert.Equal(
                DateTime.UtcNow.ToString("dd/MM/yyyy"),
                DateTime
                    .Parse(dataTable.Rows[0]["CreatedAt"].ToString() ?? "0")
                    .ToString("dd/MM/yyyy")
            );

            Assert.Equal(
                DateTime.UtcNow.ToString("dd/MM/yyyy"),
                DateTime
                    .Parse(dataTable.Rows[0]["CreatedDate"].ToString() ?? "0")
                    .ToString("dd/MM/yyyy")
            );

            Assert.Equal(DateTime.MinValue.ToString(), dataTable.Rows[0]["UpdatedAt"]);

            Assert.Equal("", dataTable.Rows[0]["UpdatedDate"]);
        }

        [Fact]
        public void ExportToExcel_ShouldCreateExcelFileWithData()
        {
            // Arrange
            var dataTable = new DataTable("Test");
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Rows.Add(1, "Item1");
            dataTable.Rows.Add(2, "Item2");

            // Act
            var excelStream = _exportService.ExportToExcel(dataTable, "Test");

            // Assert
            Assert.NotNull(excelStream);
            Assert.True(excelStream.Length > 0);

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.Worksheet("Test");
            Assert.NotNull(worksheet);
            Assert.Equal("Item1", worksheet.Cell(2, 2).Value.ToString());
        }

        [Fact]
        public void ExportMultipleToExcel_ShouldCreateExcelFileWithMultipleSheets()
        {
            // Arrange
            var dataTable1 = new DataTable("Sheet1");
            dataTable1.Columns.Add("Id", typeof(int));
            dataTable1.Columns.Add("Name", typeof(string));
            dataTable1.Rows.Add(1, "Item1");

            var dataTable2 = new DataTable("Sheet2");
            dataTable2.Columns.Add("Id", typeof(int));
            dataTable2.Columns.Add("Name", typeof(string));
            dataTable2.Rows.Add(2, "Item2");

            var dataTables = new Dictionary<string, DataTable>
            {
                { "Sheet1", dataTable1 },
                { "Sheet2", dataTable2 }
            };

            // Act
            var excelStream = _exportService.ExportMultipleToExcel(dataTables);

            // Assert
            Assert.NotNull(excelStream);
            Assert.True(excelStream.Length > 0);

            using var workbook = new XLWorkbook(excelStream);
            var worksheet1 = workbook.Worksheets.Worksheet("Sheet1");
            var worksheet2 = workbook.Worksheets.Worksheet("Sheet2");
            Assert.NotNull(worksheet1);
            Assert.NotNull(worksheet2);
            Assert.Equal("Item1", worksheet1.Cell(2, 2).Value.ToString());
            Assert.Equal("Item2", worksheet2.Cell(2, 2).Value.ToString());
        }

        [Fact]
        public void CreateZipWithExcelFiles_ShouldCreateZipWithExcelFiles()
        {
            // Arrange
            var dataTable1 = new DataTable("Sheet1");
            dataTable1.Columns.Add("Id", typeof(int));
            dataTable1.Columns.Add("Name", typeof(string));
            dataTable1.Rows.Add(1, "Item1");

            var dataTable2 = new DataTable("Sheet2");
            dataTable2.Columns.Add("Id", typeof(int));
            dataTable2.Columns.Add("Name", typeof(string));
            dataTable2.Rows.Add(2, "Item2");

            var dataTables = new Dictionary<string, DataTable>
            {
                { "Sheet1", dataTable1 },
                { "Sheet2", dataTable2 }
            };

            // Act
            MemoryStream zipStream;
            using (zipStream = _exportService.CreateZipWithExcelFiles(dataTables))
            {
                // Assert
                Assert.NotNull(zipStream);
                Assert.True(zipStream.Length > 0);

                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                Assert.Equal(2, archive.Entries.Count);
                var entry1 = archive.GetEntry("Sheet1.xlsx");
                var entry2 = archive.GetEntry("Sheet2.xlsx");
                Assert.NotNull(entry1);
                Assert.NotNull(entry2);

                using var entry1Stream = entry1.Open();
                using var workbook1 = new XLWorkbook(entry1Stream);
                var worksheet1 = workbook1.Worksheets.Worksheet("Sheet1");
                Assert.Equal("Item1", worksheet1.Cell(2, 2).Value.ToString());

                using var entry2Stream = entry2.Open();
                using var workbook2 = new XLWorkbook(entry2Stream);
                var worksheet2 = workbook2.Worksheets.Worksheet("Sheet2");
                Assert.Equal("Item2", worksheet2.Cell(2, 2).Value.ToString());
            }
        }
    }
}
