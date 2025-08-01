using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Employee.Api.Configuration;
using Employee.Api.Exceptions;
using HotChocolate;
using Microsoft.Extensions.Options;

namespace Employee.Api.Features.Employees;

[QueryType]
public class GetEmployeeQuery(
    IAmazonDynamoDB dynamoDb,
    IOptions<DynamoDbConfiguration> config,
    ILogger<GetEmployeeQuery> logger)
{
    private readonly IAmazonDynamoDB _dynamoDb = dynamoDb;
    private readonly DynamoDbConfiguration _config = config.Value;
    private readonly ILogger<GetEmployeeQuery> _logger = logger;

    public async Task<Employee.Api.Types.Employee> GetEmployee(string employeeId)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["EmployeeId"] = employeeId }))
        {
            _logger.LogInformation("Getting employee {EmployeeId}", employeeId);

            try
            {
                using var context = new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => _dynamoDb)
                    .Build();
                var employee = await context.LoadAsync<Employee.Api.Types.Employee>(employeeId);
                
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                    throw new EmployeeNotFoundException(employeeId);
                }

                return employee;
            }
            catch (EmployeeNotFoundException)
            {
                throw; // Re-throw business exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee {EmployeeId}", employeeId);
                throw new GraphQLException($"An error occurred while retrieving the employee");
            }
        }
    }
}