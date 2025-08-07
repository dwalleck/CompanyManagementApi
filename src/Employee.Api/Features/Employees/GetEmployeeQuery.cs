using Employee.Api.Data;
using Employee.Api.Exceptions;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace Employee.Api.Features.Employees;

[QueryType]
public class GetEmployeeQuery
{
    public async Task<Types.Employee> GetEmployee(
        string employeeId,
        ApplicationDbContext dbContext,
        ILogger<GetEmployeeQuery> logger)
    {
        using (logger.BeginScope(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { ["EmployeeId"] = employeeId }))
        {
            logger.LogInformation("Getting employee {EmployeeId}", employeeId);

            try
            {
                var employee = await dbContext.Employees.FindAsync(employeeId).ConfigureAwait(true);
                
                if (employee == null)
                {
                    logger.LogWarning("Employee {EmployeeId} not found", employeeId);
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
                logger.LogError(ex, "Error getting employee {EmployeeId}", employeeId);
                throw new GraphQLException($"An error occurred while retrieving the employee");
            }
        }
    }
}