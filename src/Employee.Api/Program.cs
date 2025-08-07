using FluentValidation;
using Amazon.Lambda.AspNetCoreServer;
using Employee.Api.Configuration;
using Employee.Api.Extensions;
using Employee.Api.Data;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add AWS Lambda support only when running in Lambda environment
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
{
    builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
}

// Configure PostgreSQL
builder.Services.Configure<PostgreSqlConfiguration>(builder.Configuration.GetSection("PostgreSql"));

// Add PostgreSQL DbContext with performance optimizations
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Optimize connection string for performance
var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(baseConnectionString)
{
    // Connection pooling (CRITICAL for performance)
    Pooling = true,
    MinPoolSize = 5,      // Always keep 5 connections warm
    MaxPoolSize = 100,    // Scale up to 100 connections under load
    ConnectionIdleLifetime = 300, // 5 minutes
    
    // Performance optimizations
    CommandTimeout = 30,
    Timeout = 15,
    KeepAlive = 30,
    TcpKeepAlive = true,
    
    // Disable unnecessary features for performance
    ApplicationName = "EmployeeApi",
    Enlist = true, // Enable distributed transactions if needed
};

var connectionString = connectionStringBuilder.ToString();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        
        // Performance optimizations
        npgsqlOptions.CommandTimeout(30); // 30 second timeout
    });
    
    // Critical performance settings
    options.EnableSensitiveDataLogging(false); // Never in production
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    options.EnableServiceProviderCaching();
    
    // Query performance monitoring
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging();
    }
    
    // Production-safe slow query detection
    options.ConfigureWarnings(warnings => warnings
        .Log((RelationalEventId.CommandExecuted, LogLevel.Information)));
}, ServiceLifetime.Scoped);

// DbContext health checks are handled below with the main health checks

// Register database initializer
builder.Services.AddHostedService<Employee.Api.Services.DatabaseInitializer>();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register metrics service
builder.Services.AddSingleton<Employee.Api.Services.MetricsService>();

// Add service discovery
builder.Services.AddServiceDiscovery();

// Configure HTTP client defaults with resilience and service discovery
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();
    
    // Turn on service discovery by default
    http.AddServiceDiscovery();
});

// Add health checks
builder.Services.AddHealthChecks()
    // Add a default liveness check to ensure app is responsive
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
    // Add database connectivity check
    .AddNpgSql(connectionString, name: "database", tags: ["ready"]);

// Configure GraphQL services before building the app
builder.Services
    .AddGraphQLServer()
    .AddQueryType()
        .AddTypeExtension<Employee.Api.Features.Employees.GetEmployeeQuery>()
        .AddTypeExtension<Employee.Api.Features.Employees.GetAllEmployeesQuery>()
    .AddMutationType()
        .AddTypeExtension<Employee.Api.Features.Employees.AddEmployeeCommand>()
        .AddTypeExtension<Employee.Api.Features.Employees.UpdateEmployeeCommand>()
        .AddTypeExtension<Employee.Api.Features.Employees.DeleteEmployeeCommand>()
    .AddErrorFilter<Employee.Api.ErrorHandling.GraphQLErrorFilter>()
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    })
    .InitializeOnStartup()
    .AddInstrumentation(opt =>
    {
        // Enable detailed GraphQL metrics
        opt.IncludeDocument = true;
        opt.RenameRootActivity = true;
    });

// Configure OpenTelemetry
builder.AddOpenTelemetryConfiguration();


var app = builder.Build();

// Only use HTTPS redirection when not in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGraphQL();
app.MapGet("/", () => Results.Redirect("/graphql"));

// Map health check endpoints (only in development for security)
if (app.Environment.IsDevelopment())
{
    // All health checks must pass for app to be considered ready to accept traffic after starting
    app.MapHealthChecks("/health");
    
    // Only health checks tagged with the "live" tag must pass for app to be considered alive
    app.MapHealthChecks("/alive", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live"),
    });
}
else
{
    // In production, use a simple endpoint
    app.MapGet("/health", () => Results.Ok("Healthy"));
}

app.RunWithGraphQLCommands(args);
