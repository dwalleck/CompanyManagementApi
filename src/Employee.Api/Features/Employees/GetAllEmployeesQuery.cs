using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Employee.Api.Configuration;
using HotChocolate;
using Microsoft.Extensions.Options;

namespace Employee.Api.Features.Employees;

[QueryType]
public class GetAllEmployeesQuery
{
    public async Task<IEnumerable<Employee.Api.Types.Employee>> GetEmployees(
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        ILogger<GetAllEmployeesQuery> logger)
    {
        logger.LogInformation("Getting all employees");

        try
        {
            using var context = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => dynamoDb)
                .Build();

            var employees = new List<Employee.Api.Types.Employee>();
            var search = context.ScanAsync<Employee.Api.Types.Employee>(new List<ScanCondition>());
            
            do
            {
                var batch = await search.GetNextSetAsync();
                employees.AddRange(batch);
            } while (!search.IsDone);

            logger.LogInformation("Retrieved {Count} employees", employees.Count);
            return employees;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all employees");
            throw new GraphQLException($"An error occurred while retrieving employees");
        }
    }
}