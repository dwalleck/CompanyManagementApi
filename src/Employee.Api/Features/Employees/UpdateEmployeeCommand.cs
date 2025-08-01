using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Employee.Api.Configuration;
using Employee.Api.Exceptions;
using FluentValidation;
using HotChocolate;
using Microsoft.Extensions.Options;

namespace Employee.Api.Features.Employees;

/// <summary>
/// Everything needed to update an employee in one place.
/// </summary>
[MutationType]
public class UpdateEmployeeCommand
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbConfiguration _config;
    private readonly IValidator<UpdateEmployeeInput> _validator;
    private readonly ILogger<UpdateEmployeeCommand> _logger;

    public UpdateEmployeeCommand(
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        IValidator<UpdateEmployeeInput> validator,
        ILogger<UpdateEmployeeCommand> logger)
    {
        _dynamoDb = dynamoDb;
        _config = config.Value;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Employee.Api.Types.Employee> UpdateEmployee(UpdateEmployeeInput input)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Employee.Api.Exceptions.ValidationException("Validation failed", errors);
        }

        _logger.LogInformation("Updating employee {EmployeeId}", input.EmployeeId);

        try
        {
            using var context = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => _dynamoDb)
                .Build();
            
            // Load existing employee
            var employee = await context.LoadAsync<Employee.Api.Types.Employee>(input.EmployeeId);
            if (employee == null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found for update", input.EmployeeId);
                throw new EmployeeNotFoundException(input.EmployeeId);
            }

            // Apply updates
            if (input.Name != null) employee.Name = input.Name;
            if (input.Department != null) employee.Department = input.Department;
            if (input.Salary.HasValue) employee.Salary = input.Salary.Value;
            employee.LastModified = DateTime.UtcNow;

            // Save changes
            await context.SaveAsync(employee);
            
            _logger.LogInformation("Successfully updated employee {EmployeeId}", employee.EmployeeId);
            return employee;
        }
        catch (EmployeeNotFoundException)
        {
            throw; // Re-throw business exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", input.EmployeeId);
            throw new GraphQLException($"An error occurred while updating the employee");
        }
    }
}

/// <summary>
/// Input model - specific to updating employees
/// </summary>
public class UpdateEmployeeInput
{
    public string EmployeeId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Department { get; set; }
    public decimal? Salary { get; set; }
}

/// <summary>
/// Validation for update operations
/// </summary>
public class UpdateEmployeeInputValidator : AbstractValidator<UpdateEmployeeInput>
{
    public UpdateEmployeeInputValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("Employee name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Employee name cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Department), () =>
        {
            RuleFor(x => x.Department)
                .MinimumLength(2).WithMessage("Department must be at least 2 characters long")
                .MaximumLength(50).WithMessage("Department cannot exceed 50 characters");
        });

        When(x => x.Salary.HasValue, () =>
        {
            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Salary cannot exceed 1,000,000");
        });
    }
}