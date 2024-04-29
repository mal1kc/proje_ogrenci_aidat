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
    {
        protected readonly TContext _context;
        protected readonly IConfiguration _configuration;

        protected static ILogger _logger;

        protected readonly IDbSeeder<TContext>? _dependentSeeder; // TODO: not fully implemented

        protected Random random = RandomizerHelper.random;
        protected bool _verboseLogging;

        protected DbSeeder(
            TContext context,
            IConfiguration configuration,
            ILogger logger,
            IDbSeeder<TContext>? dependentSeeder = null
        )
        {
            _context = context;
            _configuration = configuration;
            _dependentSeeder = dependentSeeder;
            _logger = logger;

            // Read the value from configuration or default to true
            _verboseLogging = _configuration.GetValue<bool>(
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

            if (_dependentSeeder != null)
            {
                await _dependentSeeder.SeedAsync(randomSeed);
            }

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

        /// <summary>
        /// <b>Important:</b> this method does not call <b>SaveChangesAsync</b>. It is the responsibility of the caller to call <b>SaveChangesAsync</b>.
        ///
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the asynchronous operation.</typeparam>
        protected abstract Task SeedEntityAsync(TEntity entity);
        protected abstract Task SeedRandomDataAsync();
        protected abstract Task AfterSeedDataAsync();

        protected abstract TEntity CreateRandomModel();

        public abstract IEnumerable<TEntity> GetSeedData(bool randomSeed = false);
    }
}
