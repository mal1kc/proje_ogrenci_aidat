using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Tests
{
    public class TestSearchStudent : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;

        private readonly ILogger<TestSearchStudent> _logger;

        private readonly IConfiguration _configuration;

        private readonly AppDbContext _dbContext;

        private readonly StudentDBSeeder _studentDBSeeder;

        private readonly StudentService _studentService;
        private bool disposedValue;

        private readonly List<Student> _seedData;

        private readonly QueryableModelHelper<Student> studentSearchHelper;

        public TestSearchStudent()
        {
            // Use an in-memory database for testing

            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _configuration = Helpers.CreateConfiguration();

            _dbContext = new AppDbContext(_options);

            // Seed the database with test data

            _studentDBSeeder = new StudentDBSeeder(
                context: _dbContext,
                configuration: _configuration,
                logger: Helpers.CreateLogger<StudentDBSeeder>(),
                studentService: _studentService,
                randomSeed: true
            );

            // Add more students as needed
            _dbContext.SaveChanges();

            _seedData = _studentDBSeeder.GetSeedData().ToList();
            _seedData ??= [];
            School school =
                new() { Name = "Test School", Students = new HashSet<Student?>(_seedData) };

            foreach (var seedEntity in _seedData)
            {
                seedEntity.EmailAddress = string.Concat(
                    Path.GetRandomFileName().Replace(".", "").AsSpan(0, 10),
                    "@random.com"
                );
                seedEntity.StudentId = Guid.NewGuid().ToString().Replace("-", "")[..10];
            }

            studentSearchHelper = new QueryableModelHelper<Student>(
                _seedData.AsQueryable(),
                Student.SearchConfig
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~TestSearchStudent()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // test random created data can be searchable check for fails
        [Fact]
        public async Task SearchRandomDoesNotFail()
        {
            await _studentDBSeeder.SeedAsync();

            // Arrange
            var queryable = _dbContext.Students.AsQueryable();

            QueryableModelHelper<Student> studentSearchHelper =
                new(queryable, Student.SearchConfig);
            // random str in lenght of 3-10
            var random = new Random();
            var length = random.Next(3, 11);
            var randomStr = Path.GetRandomFileName().Replace(".", "").Substring(0, length);

            // Act
            var result = studentSearchHelper.Search(randomStr);

            foreach (var field in Student.SearchConfig.AllowedFieldsForSearch)
            {
                result = studentSearchHelper.Search(randomStr);
            }
        }

        [Fact]
        public Task SearchRandomDataWithSpecifiedField()
        {
            foreach (var seedEntity in _seedData)
            {
                foreach (var searchField in new[] { "FirstName", "LastName", "EmailAddress" })
                {
                    // check if search field can return correct result
                    var property =
                        seedEntity.GetType().GetProperty(searchField)
                        ?? throw new Exception(
                            $"Property '{searchField}' does not exist on type '{seedEntity.GetType().Name}'."
                        );
                    var searchValue = property.GetValue(seedEntity, null);
                    if (searchValue == null)
                        continue;
                    var searchResult = studentSearchHelper.Search(
                        searchValue.ToString(),
                        searchField
                    );

                    Assert.NotNull(searchResult);

                    Assert.True(searchResult.Any(), $"Search result is empty for {searchField}");

                    Assert.Equal(seedEntity.EmailAddress, searchResult.First().EmailAddress);
                }
            }

            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchRandomDataWithoutField()
        {
            foreach (var seedEntity in _seedData)
            {
                foreach (var searchField in new[] { "FirstName", "LastName", "EmailAddress" })
                {
                    // check if search field can return correct result
                    var property =
                        seedEntity.GetType().GetProperty(searchField)
                        ?? throw new Exception(
                            $"Property '{searchField}' does not exist on type '{seedEntity.GetType().Name}'."
                        );
                    var searchValue = property.GetValue(seedEntity, null);
                    if (searchValue == null)
                        continue;
                    var searchResult = studentSearchHelper.Search(searchValue.ToString());

                    Assert.NotNull(searchResult);

                    Assert.True(
                        searchResult.Any(),
                        $"Search result is empty for {searchValue} at {searchField} field."
                    );

                    Assert.True(searchResult.Any(s => s.EmailAddress == seedEntity.EmailAddress));

                    Assert.NotEqual(_seedData.Count, searchResult.Count());
                }
            }

            return Task.CompletedTask;
        }
    }
}
