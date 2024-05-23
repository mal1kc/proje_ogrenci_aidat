using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Tests
{
    public class TestSearchStudent : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;

        private readonly ILogger<TestSearchStudent> _logger;

        private readonly IConfiguration _configuration;

        private readonly AppDbContext _dbContext;

        private readonly StudentDBSeeder _studentDBSeeder;
        private bool disposedValue;

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
                logger: Helpers.CreateLogger<StudentDBSeeder>()
            );

            // Add more students as needed
            _dbContext.SaveChanges();
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
            await _studentDBSeeder.SeedAsync(randomSeed: true);

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
        public Task SearchRandomData()
        {

            var seedData = _studentDBSeeder.GetSeedData(true);

            QueryableModelHelper<Student> studentSearchHelper =
                new(seedData.AsQueryable(), Student.SearchConfig);

            Exception? exception = null;
            foreach (var seedEntity in seedData)
            {
                foreach (var searchField in Student.SearchConfig.AllowedFieldsForSearch)
                {
                    // check if search field can return correct result
                    var property =
                        seedEntity.GetType().GetProperty(searchField)
                        ?? throw new Exception(
                            $"Property '{searchField}' does not exist on type '{seedEntity.GetType().Name}'."
                        );
                    var searchValue = property.GetValue(seedEntity, null);
                    if (searchValue == null) continue;
                    var searchResult = studentSearchHelper.Search(searchValue.ToString(), searchField);

                    // we use Firstname because id isn't generated yet(we did not add to db)
                    exception = new Exception(
                             $"No students found with {searchField} equal to '{searchValue}'."
                         );
                    if (!searchResult.Any()) throw exception;

                    Assert.Equal(seedEntity.EmailAddress, searchResult.First().EmailAddress);

                    // search without specifying field
                    searchResult = studentSearchHelper.Search(searchValue.ToString());

                    if (!searchResult.Any()) throw exception;
                    Assert.Equal(seedEntity.EmailAddress, searchResult.First().EmailAddress);
                }
            }

            return Task.CompletedTask;
        }

    }
}
