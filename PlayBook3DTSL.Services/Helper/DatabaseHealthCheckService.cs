using System;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace PlayBook3DTSL.Services.Helper
{
    public class DatabaseHealthCheckService : BackgroundService
    {
        private readonly ILogger<DatabaseHealthCheckService> _logger;
        private readonly string _connectionString;
        private readonly AsyncRetryPolicy _retryPolicy;

        public DatabaseHealthCheckService(ILogger<DatabaseHealthCheckService> logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;

            // Define Polly retry policy with exponential backoff (max 3 retries)
            _retryPolicy = Policy
                .Handle<SqlException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Database connection attempt {retryCount} failed. Waiting {timeSpan} before next try. Exception: {exception.Message}");
                    });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Health Check Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting database connectivity check...");

                try
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        using var connection = new SqlConnection(_connectionString);
                        await connection.OpenAsync();
                        _logger.LogInformation("Successfully connected to the database.");
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database connectivity check failed after retries.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Database Health Check Service is stopping.");
        }
    }
}
