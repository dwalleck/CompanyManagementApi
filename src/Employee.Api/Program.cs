using FluentValidation;
using Amazon.DynamoDBv2;
using Amazon.Lambda.AspNetCoreServer;
using Employee.Api.Configuration;
using Employee.Api.Extensions;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add AWS Lambda support only when running in Lambda environment
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
{
    builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
}

// Configure DynamoDB
builder.Services.Configure<DynamoDbConfiguration>(builder.Configuration.GetSection("DynamoDb"));

// Configure AWS options based on environment
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL_DYNAMODB")))
{
    // Running with Aspire - use local DynamoDB with dummy credentials
    var awsOptions = builder.Configuration.GetAWSOptions();
    awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials("local", "local");
    builder.Services.AddDefaultAWSOptions(awsOptions);
}
else
{
    // Running in AWS - use normal credential chain
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
}

// Add DynamoDB client
// When running with Aspire, the AWS_ENDPOINT_URL_DYNAMODB environment variable
// will be set automatically by the WithReference call
builder.Services.AddAWSService<IAmazonDynamoDB>();

// Register DynamoDB table initializer
builder.Services.AddHostedService<Employee.Api.Services.DynamoDbInitializer>();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

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
    .AddInstrumentation();

// Configure OpenTelemetry
builder.AddOpenTelemetryConfiguration();


var app = builder.Build();

// Only use HTTPS redirection when not in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Map GraphQL endpoint
app.MapGraphQL();

// Add a simple health check endpoint for Aspire
app.MapGet("/", () => Results.Redirect("/graphql"));

// Map health check endpoints (only in development for security)
if (app.Environment.IsDevelopment())
{
    // All health checks must pass for app to be considered ready to accept traffic after starting
    app.MapHealthChecks("/health");
    
    // Only health checks tagged with the "live" tag must pass for app to be considered alive
    app.MapHealthChecks("/alive", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });
}
else
{
    // In production, use a simple endpoint
    app.MapGet("/health", () => Results.Ok("Healthy"));
}

app.RunWithGraphQLCommands(args);
