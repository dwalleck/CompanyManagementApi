using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Employee.Api.Configuration;
using Employee.Api.Exceptions;
using HotChocolate;
using Microsoft.Extensions.Options;

namespace Employee.Api.Features.Employees;

[MutationType]
public class DeleteEmployeeCommand
{
    public async Task<bool> DeleteEmployee(
        string employeeId,
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        ILogger<DeleteEmployeeCommand> logger)
    {
        logger.LogInformation("Deleting employee {EmployeeId}", employeeId);

        try
        {
            using var context = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => dynamoDb)
                .Build();
            
            // Check if employee exists
            var employee = await context.LoadAsync<Employee.Api.Types.Employee>(employeeId);
            if (employee == null)
            {
                logger.LogWarning("Employee {EmployeeId} not found for deletion", employeeId);
                throw new EmployeeNotFoundException(employeeId);
            }

            // Delete employee
            await context.DeleteAsync<Employee.Api.Types.Employee>(employeeId);
            
            logger.LogInformation("Successfully deleted employee {EmployeeId}", employeeId);
            return true;
        }
        catch (EmployeeNotFoundException)
        {
            throw; // Re-throw business exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting employee {EmployeeId}", employeeId);
            throw new GraphQLException($"An error occurred while deleting the employee");
        }
    }
}