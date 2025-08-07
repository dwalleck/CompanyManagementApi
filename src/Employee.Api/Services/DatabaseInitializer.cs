using Microsoft.EntityFrameworkCore;
using Employee.Api.Data;

namespace Employee.Api.Services;

public class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IHostEnvironment _environment;

    public DatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializer> logger,
        IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _logger.LogInformation("Ensuring database exists and is up to date");

            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("Running in development mode - applying migrations");
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("Running in production mode - ensuring database is created");
                var created = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                if (created)
                {
                    _logger.LogInformation("Database created successfully");
                }
                else
                {
                    _logger.LogInformation("Database already exists");
                }
            }

            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                _logger.LogInformation("Successfully connected to PostgreSQL database");
            }
            else
            {
                _logger.LogWarning("Unable to connect to PostgreSQL database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing PostgreSQL database");
            if (!_environment.IsDevelopment())
            {
                throw;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}