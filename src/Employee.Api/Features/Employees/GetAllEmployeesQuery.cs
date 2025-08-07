using Employee.Api.Data;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace Employee.Api.Features.Employees;

[QueryType]
public class GetAllEmployeesQuery
{
    public async Task<IEnumerable<Employee.Api.Types.Employee>> GetEmployees(
        ApplicationDbContext dbContext,
        ILogger<GetAllEmployeesQuery> logger)
    {
        logger.LogInformation("Getting all employees");

        try
        {
            var employees = await dbContext.Employees.ToListAsync();
            
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