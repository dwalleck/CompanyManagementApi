using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Employee.Api.Configuration;
using FluentValidation;
using HotChocolate;
using Microsoft.Extensions.Options;

namespace Employee.Api.Features.Employees;

/// <summary>
/// Everything needed to add an employee in one place.
/// No need to jump between folders to understand this feature.
/// </summary>
[MutationType]
public class AddEmployeeCommand
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbConfiguration _config;
    private readonly IValidator<AddEmployeeInput> _validator;
    private readonly ILogger<AddEmployeeCommand> _logger;

    public AddEmployeeCommand(
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        IValidator<AddEmployeeInput> validator,
        ILogger<AddEmployeeCommand> logger)
    {
        _dynamoDb = dynamoDb;
        _config = config.Value;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Employee.Api.Types.Employee> AddEmployee(AddEmployeeInput input)
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

        var employee = new Employee.Api.Types.Employee
        {
            EmployeeId = Guid.NewGuid().ToString(),
            Name = input.Name,
            Department = input.Department,
            Salary = input.Salary,
            HireDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        _logger.LogInformation("Creating employee {EmployeeId} - {Name}", employee.EmployeeId, employee.Name);

        try
        {
            using var context = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => _dynamoDb)
                .Build();
            await context.SaveAsync(employee);
            
            _logger.LogInformation("Successfully created employee {EmployeeId}", employee.EmployeeId);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            throw new GraphQLException($"An error occurred while creating the employee");
        }
    }
}

/// <summary>
/// Input model - specific to this feature
/// </summary>
public class AddEmployeeInput
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal Salary { get; set; }
}

/// <summary>
/// Validation - lives with the feature that uses it
/// </summary>
public class AddEmployeeInputValidator : AbstractValidator<AddEmployeeInput>
{
    public AddEmployeeInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Employee name is required")
            .MinimumLength(2).WithMessage("Employee name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Employee name cannot exceed 100 characters");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MinimumLength(2).WithMessage("Department must be at least 2 characters long")
            .MaximumLength(50).WithMessage("Department cannot exceed 50 characters");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Salary cannot exceed 1,000,000");
    }
}