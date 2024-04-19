using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Data
{
    public interface IDbSeeder<TContext>
        where TContext : DbContext
    {
        Task SeedAsync(bool randomSeed = false);
        Task AfterSeedAsync();
    }

    public abstract class DbSeeder<TContext, TEntity> : IDbSeeder<TContext>
        where TContext : DbContext
        where TEntity : class, IBaseDbModel
    {
        protected readonly TContext _context;
        protected readonly IConfiguration Configuration;

        /* protected Random random = new Random(); */
        protected Random random = RandomizerHelper.random;
        protected bool _verboseLogging;

        protected DbSeeder(TContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;

            // Read the value from configuration or default to true
            _verboseLogging = Configuration.GetValue<bool>(
                "SeedData:VerboseLogging",
                defaultValue: true
            );
        }

        public async Task SeedAsync(bool randomSeed = false)
        {
            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Database connection is not available.");
            }

            await _context.Database.EnsureCreatedAsync();

            if (randomSeed)
            {
                await SeedRandomDataAsync();
            }
            else
            {
                await SeedDataAsync();
            }
        }

        public async Task AfterSeedAsync()
        {
            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Database connection is not available.");
            }

            await _context.Database.EnsureCreatedAsync();
            await AfterSeedDataAsync();
        }

        protected abstract Task SeedDataAsync();
        protected abstract Task SeedRandomDataAsync();
        protected abstract Task AfterSeedDataAsync();

        protected abstract TEntity CreateRandomModel();
    }
}
