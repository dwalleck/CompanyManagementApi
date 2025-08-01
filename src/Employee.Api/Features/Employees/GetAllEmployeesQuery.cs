using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Employee.Api.Configuration;
using HotChocolate;
using Microsoft.Extensions.Options;

namespace Employee.Api.Features.Employees;

[QueryType]
public class GetAllEmployeesQuery
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbConfiguration _config;
    private readonly ILogger<GetAllEmployeesQuery> _logger;

    public GetAllEmployeesQuery(
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        ILogger<GetAllEmployeesQuery> logger)
    {
        _dynamoDb = dynamoDb;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<Employee.Api.Types.Employee>> GetEmployees()
    {
        _logger.LogInformation("Getting all employees");

        try
        {
            using var context = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => _dynamoDb)
                .Build();

            var employees = new List<Employee.Api.Types.Employee>();
            var search = context.ScanAsync<Employee.Api.Types.Employee>(new List<ScanCondition>());
            
            do
            {
                var batch = await search.GetNextSetAsync();
                employees.AddRange(batch);
            } while (!search.IsDone);

            _logger.LogInformation("Retrieved {Count} employees", employees.Count);
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all employees");
            throw new GraphQLException($"An error occurred while retrieving employees");
        }
    }
}