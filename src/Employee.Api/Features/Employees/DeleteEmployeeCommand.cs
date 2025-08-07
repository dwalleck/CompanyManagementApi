using Employee.Api.Data;
using Employee.Api.Exceptions;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace Employee.Api.Features.Employees;

[MutationType]
public class DeleteEmployeeCommand
{
    public async Task<bool> DeleteEmployee(
        string employeeId,
        ApplicationDbContext dbContext,
        ILogger<DeleteEmployeeCommand> logger)
    {
        logger.LogInformation("Deleting employee {EmployeeId}", employeeId);

        try
        {
            // Check if employee exists
            var employee = await dbContext.Employees.FindAsync(employeeId).ConfigureAwait(true);
            if (employee == null)
            {
                logger.LogWarning("Employee {EmployeeId} not found for deletion", employeeId);
                throw new EmployeeNotFoundException(employeeId);
            }

            // Delete employee
            dbContext.Employees.Remove(employee);
            await dbContext.SaveChangesAsync().ConfigureAwait(true);
            
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