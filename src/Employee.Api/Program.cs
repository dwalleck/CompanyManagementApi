using FluentValidation;
using Amazon.DynamoDBv2;
using Amazon.Lambda.AspNetCoreServer;
using Employee.Api.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Instrumentation.AWSLambda;
using Amazon.Extensions.NETCore.Setup;

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
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("Employee.Api")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["service.namespace"] = "CompanyManagement",
                    ["deployment.environment"] = builder.Environment.EnvironmentName
                }))
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHotChocolateInstrumentation()
            .AddAWSInstrumentation()
            .AddAWSLambdaConfigurations()
            .AddOtlpExporter(options =>
            {
                // AWS Lambda will set the OTEL_EXPORTER_OTLP_ENDPOINT environment variable
                // when using AWS Lambda Telemetry API
            });
    });

// Configure OpenTelemetry Logging
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Employee.Api"));
});


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
app.MapGet("/health", () => Results.Ok("Healthy"));

app.RunWithGraphQLCommands(args);
