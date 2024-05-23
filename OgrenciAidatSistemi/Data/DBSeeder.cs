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
        protected bool _is_seeding;
        protected readonly TContext _context;
        protected readonly IConfiguration _configuration;

        protected static ILogger _logger;

        protected int _maxSeedCount = 10;

        protected int _seedCount = 0; // to keep track of the number of entities seeded

        protected readonly IDbSeeder<TContext>? _dependentSeeder; // TODO: not fully implemented

        protected Random random = RandomizerHelper.random;
        protected bool _verboseLogging;

        protected DbSeeder(
            TContext context,
            IConfiguration configuration,
            ILogger logger,
            int maxSeedCount = 100,
            IDbSeeder<TContext>? dependentSeeder = null
        )
        {
            _context = context;
            _configuration = configuration;
            _dependentSeeder = dependentSeeder;
            _logger = logger;
            _maxSeedCount = _configuration.GetValue<int>(
                "SeedData:MaxSeedCount",
                defaultValue: maxSeedCount
            );
            _seedCount = 0;

            // Read the value from configuration or default to true
            _verboseLogging = _configuration.GetValue<bool>(
                "SeedData:VerboseLogging",
                defaultValue: true
            );

            _is_seeding = configuration.GetSection("SeedData").GetValue("SeedDB", false);
        }

        public async Task SeedAsync(bool randomSeed = false)
        {
            if (!_is_seeding)
                return;

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
            if (!_is_seeding)
                return;

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
        public abstract Task SeedEntityAsync(TEntity entity);
        protected abstract Task SeedRandomDataAsync();
        protected abstract Task AfterSeedDataAsync();

        protected abstract TEntity CreateRandomModel();

        public abstract IEnumerable<TEntity> GetSeedData(bool randomSeed = false);
    }
}
