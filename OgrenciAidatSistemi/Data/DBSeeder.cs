using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Data
{
    public interface IDbSeeder<TContext>
        where TContext : DbContext
    {
        Task SeedAsync();
        Task SeedRandomAsync();
        Task AfterSeedAsync();
    }

    public abstract class DbSeeder<TContext, TEntity> : IDbSeeder<TContext>
        where TContext : DbContext
        where TEntity : IBaseDbModel
    {
        public readonly IConfiguration Configuration;
        protected readonly TContext _context;
        public List<TEntity> _seedData;
        protected bool _verboselogging;

        protected DbSeeder(TContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;

            // Read the value from configuration or default to true
            _verboselogging = Configuration.GetValue<bool>(
                "SeedData:VerboseLogging",
                defaultValue: true
            );
        }

        public async Task SeedAsync()
        {
            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Database connection is not available.");
            }
            _ = await _context.Database.EnsureCreatedAsync();

            await SeedDataAsync();
        }

        public async Task SeedRandomAsync()
        {
            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Database connection is not available.");
            }

            _ = await _context.Database.EnsureCreatedAsync();

            await SeedRandomDataAsync();
        }

        public async Task AfterSeedAsync()
        {
            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Database connection is not available.");
            }

            _ = await _context.Database.EnsureCreatedAsync();
            await AfterSeedDataAsync();
        }

        protected abstract Task SeedDataAsync();
        protected abstract Task SeedRandomDataAsync();
        protected abstract Task AfterSeedDataAsync();
    }
}
