using Microsoft.EntityFrameworkCore;
using Moq;
using OgrenciAidatSistemi.Services;
using OgrenciAidatSistemi.Tests.Data;

namespace OgrenciAidatSistemi.Tests
{
    public class ExportServiceStaticTests
    {
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
                    CreatedAt = DateTime.Now,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    Amount = 100.0m
                },
                new()
                {
                    Id = 2,
                    Name = "Item2",
                    CreatedAt = DateTime.Now,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
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
                    CreatedAt = DateTime.Now,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    Amount = 100.0m
                },
                new()
                {
                    Id = 2,
                    Name = "Item2",
                    CreatedAt = DateTime.Now,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
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
                    CreatedAt = DateTime.Now,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    Amount = 100.0m,
                    TestSeconds = new TestSecondItem { Name = "Related1" }
                },
                new()
                {
                    Id = 2,
                    Name = "Item2",
                    CreatedAt = DateTime.Now,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
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
                DateTime.Now.ToString("dd/MM/yyyy"),
                DateTime
                    .Parse(dataTable.Rows[0]["CreatedAt"].ToString() ?? "0")
                    .ToString("dd/MM/yyyy")
            );

            Assert.Equal(
                DateTime.Now.ToString("dd/MM/yyyy"),
                DateTime
                    .Parse(dataTable.Rows[0]["CreatedDate"].ToString() ?? "0")
                    .ToString("dd/MM/yyyy")
            );

            Assert.Equal(DateTime.MinValue.ToString(), dataTable.Rows[0]["UpdatedAt"]);

            Assert.Equal("", dataTable.Rows[0]["UpdatedDate"]);
        }
    }
}
