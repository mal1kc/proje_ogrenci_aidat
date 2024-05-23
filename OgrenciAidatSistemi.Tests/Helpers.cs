using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OgrenciAidatSistemi.Tests
{
    public static class Helpers
    {
        public static IConfiguration CreateConfiguration()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Add any configuration values needed for testing

            return configuration;
        }

        internal static ILogger<T> CreateLogger<T>()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            return loggerFactory.CreateLogger<T>();
        }
    }
}
