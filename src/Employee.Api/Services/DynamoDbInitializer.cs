using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Employee.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Employee.Api.Services;

public class DynamoDbInitializer : IHostedService
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbConfiguration _config;
    private readonly ILogger<DynamoDbInitializer> _logger;
    private readonly IHostEnvironment _environment;

    public DynamoDbInitializer(
        IAmazonDynamoDB dynamoDb, 
        IOptions<DynamoDbConfiguration> config,
        ILogger<DynamoDbInitializer> logger,
        IHostEnvironment environment)
    {
        _dynamoDb = dynamoDb;
        _config = config.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        try
        {
            // Check if table exists
            var tables = await _dynamoDb.ListTablesAsync(cancellationToken);
            if (tables.TableNames.Contains(_config.TableName))
            {
                _logger.LogInformation("Table {TableName} already exists", _config.TableName);
                return;
            }

            // Create table
            _logger.LogInformation("Creating DynamoDB table {TableName}", _config.TableName);
            
            var request = new CreateTableRequest
            {
                TableName = _config.TableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "EmployeeId",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "EmployeeId",
                        KeyType = "HASH"
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            await _dynamoDb.CreateTableAsync(request, cancellationToken);
            _logger.LogInformation("Successfully created table {TableName}", _config.TableName);

            // Wait for table to be active
            await WaitForTableToBeActive(_config.TableName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing DynamoDB table");
        }
    }

    private async Task WaitForTableToBeActive(string tableName, CancellationToken cancellationToken)
    {
        var maxWaitTime = TimeSpan.FromSeconds(30);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await _dynamoDb.DescribeTableAsync(tableName, cancellationToken);
            if (response.Table.TableStatus == TableStatus.ACTIVE)
            {
                _logger.LogInformation("Table {TableName} is now active", tableName);
                return;
            }

            await Task.Delay(1000, cancellationToken);
        }

        _logger.LogWarning("Table {TableName} did not become active within timeout", tableName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}