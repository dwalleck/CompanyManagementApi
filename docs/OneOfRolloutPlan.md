# OneOf Pattern Rollout Plan for Vertical Slice Architecture

## Overview
This document outlines a phased approach to adopting the OneOf pattern across the Employee API project, replacing exception-based error handling with explicit error types while maintaining the Vertical Slice Architecture.

## Phase 1: Foundation (Week 1)
**Goal**: Set up infrastructure and validate the pattern with one feature

### Tasks:
1. ✅ **Already Complete**:
   - `Error.cs` class defined
   - `OneOfExtensions.cs` with GraphQL integration
   - Method injection refactoring done

2. **Clean Up**:
   - Delete unused `Result.cs` and `ResultExtensions.cs`
   - Delete `ResultTestExtensions.cs` (create test helpers when needed)
   - Delete example files (`DirectOneOfExample.cs`, etc.)
   - Update `CLAUDE.md` with OneOf pattern guidance

3. **Pilot Feature - GetEmployeeQuery**:
   - Start with the simplest query operation
   - Keep all logic in the feature file (VSA principle)
   - Validate the pattern works end-to-end

### Example Migration for Vertical Slice:
```csharp
// GetEmployeeQuery.cs - BEFORE
[QueryType]
public class GetEmployeeQuery
{
    public async Task<Employee> GetEmployee(
        string employeeId,
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        ILogger<GetEmployeeQuery> logger)
    {
        using (logger.BeginScope(new Dictionary<string, object> { ["EmployeeId"] = employeeId }))
        {
            logger.LogInformation("Getting employee {EmployeeId}", employeeId);

            try
            {
                using var context = new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => dynamoDb)
                    .Build();
                var employee = await context.LoadAsync<Employee>(employeeId);
                
                if (employee == null)
                {
                    logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                    throw new EmployeeNotFoundException(employeeId);
                }

                return employee;
            }
            catch (EmployeeNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting employee {EmployeeId}", employeeId);
                throw new GraphQLException($"An error occurred while retrieving the employee");
            }
        }
    }
}

// GetEmployeeQuery.cs - AFTER with OneOf
[QueryType]
public class GetEmployeeQuery
{
    public async Task<OneOf<Employee, Error>> GetEmployee(
        string employeeId,
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        ILogger<GetEmployeeQuery> logger)
    {
        using (logger.BeginScope(new Dictionary<string, object> { ["EmployeeId"] = employeeId }))
        {
            logger.LogInformation("Getting employee {EmployeeId}", employeeId);

            try
            {
                using var context = new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => dynamoDb)
                    .Build();
                var employee = await context.LoadAsync<Employee>(employeeId);
                
                if (employee == null)
                {
                    logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                    return new Error(
                        $"Employee with ID '{employeeId}' not found",
                        "EMPLOYEE_NOT_FOUND",
                        new Dictionary<string, object> { ["employeeId"] = employeeId });
                }

                return employee;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting employee {EmployeeId}", employeeId);
                return new Error(
                    "An error occurred while retrieving the employee",
                    "DATABASE_ERROR",
                    new Dictionary<string, object> { ["type"] = ex.GetType().Name });
            }
        }
    }
}
```

## Phase 2: Query Operations (Week 2)
**Goal**: Convert remaining query operations to use OneOf

### Tasks:
1. Convert `GetAllEmployeesQuery` using the same pattern
2. Decision point: nullable returns vs throwing exceptions

### GetAllEmployeesQuery Migration Pattern:
```csharp
[QueryType]
public class GetAllEmployeesQuery
{
    public async Task<OneOf<IEnumerable<Employee>, Error>> GetEmployees(
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

            var employees = new List<Employee>();
            var search = context.ScanAsync<Employee>(new List<ScanCondition>());
            
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
            return new Error(
                "An error occurred while retrieving employees",
                "DATABASE_ERROR",
                new Dictionary<string, object> { ["operation"] = "scan" });
        }
    }
}
```

## Phase 3: Mutation Operations (Week 3)
**Goal**: Convert all mutations to use OneOf

### Priority Order:
1. `DeleteEmployeeCommand` - Simplest, returns bool
2. `AddEmployeeCommand` - Has validation complexity
3. `UpdateEmployeeCommand` - Most complex with partial updates

### Pattern for Each Mutation:
```csharp
[MutationType]
public class AddEmployeeCommand
{
    public async Task<OneOf<Employee, Error>> AddEmployee(
        AddEmployeeInput input,
        IAmazonDynamoDB dynamoDb,
        IOptions<DynamoDbConfiguration> config,
        IValidator<AddEmployeeInput> validator,
        ILogger<AddEmployeeCommand> logger)
    {
        // Validate input
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            return validationResult.ToOneOf(input).AsT1;
        }

        var employee = new Employee
        {
            EmployeeId = Guid.NewGuid().ToString(),
            Name = input.Name,
            Department = input.Department,
            Salary = input.Salary,
            HireDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        logger.LogInformation("Creating employee {EmployeeId} - {Name}", employee.EmployeeId, employee.Name);

        try
        {
            using var context = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => dynamoDb)
                .Build();
            await context.SaveAsync(employee);
            
            logger.LogInformation("Successfully created employee {EmployeeId}", employee.EmployeeId);
            return employee;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating employee");
            return new Error(
                "An error occurred while creating the employee",
                "DATABASE_ERROR",
                new Dictionary<string, object> 
                { 
                    ["operation"] = "create",
                    ["type"] = ex.GetType().Name 
                });
        }
    }
}
```

## Phase 4: Patterns and Refinement (Week 4)
**Goal**: Establish patterns and refine approach

### Key Patterns to Establish:

1. **Validation Pattern**:
```csharp
// Consistent validation handling
var validationResult = await validator.ValidateAsync(input);
if (!validationResult.IsValid)
{
    return validationResult.ToOneOf(input).AsT1;
}
```

2. **Error Code Standards**:
- `VALIDATION_ERROR` - Input validation failed
- `{ENTITY}_NOT_FOUND` - Entity doesn't exist  
- `DATABASE_ERROR` - Database operation failed
- `BUSINESS_RULE_ERROR` - Business rule violation

3. **Logging Pattern**:
- Log at operation start with context
- Log warnings for business errors (not found, validation)
- Log errors for technical failures
- Include operation type in error details

4. **Composition Example** (if needed):
```csharp
public async Task<OneOf<Employee, Error>> CreateAndNotifyAsync(
    AddEmployeeInput input,
    /* dependencies */)
{
    // Create employee
    var createResult = await AddEmployee(input, /* deps */);
    if (createResult.IsT1) return createResult.AsT1;
    
    var employee = createResult.AsT0;
    
    // Send notification (don't fail if this fails)
    try
    {
        await SendNotificationAsync(employee);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to send notification for {EmployeeId}", employee.EmployeeId);
    }
    
    return employee;
}
```

## Testing Approach

### Unit Test Pattern:
```csharp
public class GetEmployeeQueryTests
{
    [Fact]
    public async Task GetEmployee_WhenExists_ReturnsEmployee()
    {
        // Arrange
        var dynamoDb = CreateMockDynamoDB();
        var logger = new NullLogger<GetEmployeeQuery>();
        var query = new GetEmployeeQuery();
        
        // Act
        var result = await query.GetEmployee("123", dynamoDb, config, logger);
        
        // Assert
        Assert.True(result.IsT0);
        var employee = result.AsT0;
        Assert.Equal("123", employee.EmployeeId);
    }

    [Fact]
    public async Task GetEmployee_WhenNotFound_ReturnsError()
    {
        // Arrange
        var dynamoDb = CreateMockDynamoDB();
        var logger = new NullLogger<GetEmployeeQuery>();
        var query = new GetEmployeeQuery();
        
        // Act
        var result = await query.GetEmployee("nonexistent", dynamoDb, config, logger);
        
        // Assert  
        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal("EMPLOYEE_NOT_FOUND", error.Code);
        Assert.Contains("nonexistent", error.Details["employeeId"].ToString());
    }
}
```

### Integration Test Considerations:
- GraphQL endpoints should continue to work unchanged
- Error responses should contain proper extensions
- Partial success scenarios (with ResolveOrReport) should be tested

## Decision Points

### 1. Error Handling Strategy
The GraphQL resolvers will return OneOf<T, Error> directly. Hot Chocolate will handle the error reporting through the OneOfExtensions:

```csharp
public async Task<OneOf<Employee, Error>> GetEmployee(input, deps)
{
    // Implementation returns OneOf directly
}
```

This approach leverages Hot Chocolate's ability to handle union types and the custom extensions for error reporting.

### 2. Method Organization
Since we're using VSA, each feature file will have:
- Single public resolver method that returns OneOf<T, Error>
- All logic contained within that method
- Keep everything in one file

### 3. Testing Strategy
- Test the public resolver methods directly
- Mock dependencies as needed
- Verify both success and error cases

## Implementation Checklist

### Phase 1: Foundation (1-2 days)
- [ ] Clean up unused Result files
- [ ] Update CLAUDE.md with OneOf guidance
- [ ] Convert GetEmployeeQuery as pilot
- [ ] Write unit tests for GetEmployeeQuery
- [ ] Validate pattern works end-to-end

### Phase 2: Remaining Queries (2-3 days)
- [ ] Convert GetAllEmployeesQuery
- [ ] Decide on error handling approach
- [ ] Update tests

### Phase 3: Mutations (3-4 days)
- [ ] Convert DeleteEmployeeCommand (simplest)
- [ ] Convert AddEmployeeCommand
- [ ] Convert UpdateEmployeeCommand
- [ ] Ensure validation errors are structured properly

### Phase 4: Polish (2-3 days)
- [ ] Document error codes
- [ ] Create team guidance
- [ ] Review and refine patterns
- [ ] Consider composition scenarios

## Key Principles for VSA + OneOf

1. **Keep it in the slice**: All logic stays in the feature file
2. **Single method pattern**: Public resolver returns OneOf directly
3. **Gradual migration**: Start with one feature, validate, then continue
4. **No service layer**: VSA means no separate service classes
5. **Consistent patterns**: Use the same approach across all features

## Risk Mitigation

- Start with read operations (less risky)
- Keep resolver signatures unchanged initially
- Can always revert individual features
- OneOf is just a return type - easy to change

## Success Metrics

- All features use OneOf internally
- Zero regression in functionality
- Improved error information in logs
- Team finds the pattern helpful (not burdensome)