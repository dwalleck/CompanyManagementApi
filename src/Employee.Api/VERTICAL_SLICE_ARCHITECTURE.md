# Vertical Slice Architecture in Employee API

This Employee API follows Vertical Slice Architecture (VSA) principles, organizing code by features rather than technical layers.

## Structure

```
Features/
└── Employees/
    ├── GetEmployeeQuery.cs           # Query + all related code
    ├── GetAllEmployeesQuery.cs       # Query + all related code
    ├── AddEmployeeCommand.cs         # Command + Input + Validator
    ├── UpdateEmployeeCommand.cs      # Command + Input + Validator
    └── DeleteEmployeeCommand.cs      # Command + all related code
```

No unnecessary subdirectories - just feature files grouped by aggregate (Employees).

## Benefits

### 1. **Feature Cohesion**
Each feature folder contains everything needed for that use case:
- GraphQL endpoint definition
- Business logic
- Data access
- Validation rules (when needed)
- Logging and error handling

### 2. **Easy Navigation**
Need to fix a bug in employee creation? Look in `Features/Employees/AddEmployee/`. Everything is there.

### 3. **Independent Features**
Each feature can:
- Use different data access patterns (direct DynamoDB vs repository)
- Have its own optimization strategies
- Be tested independently
- Be modified without affecting other features

### 4. **Simplified Code**
No unnecessary layers or abstractions. Each feature uses exactly what it needs:
- GetEmployee: Direct DynamoDB LoadAsync
- GetAllEmployees: DynamoDB ScanAsync with pagination
- AddEmployee: Includes validation
- UpdateEmployee: Partial updates with validation
- DeleteEmployee: Simple delete with existence check

### 5. **GraphQL Integration**
Uses Hot Chocolate's type extension pattern:
- `[QueryType]` for queries
- `[MutationType]` for mutations
- Registered as type extensions in Program.cs

## Adding New Features

1. Add a new file under `Features/Employees/` (or create a new aggregate folder)
2. In that single file, include:
   - Your Query/Command class with `[QueryType]` or `[MutationType]`
   - Input models
   - Validators
   - All business logic
3. Register as type extension in Program.cs
4. That's it! No other files or folders needed!

## Example: Adding Employee Search

```csharp
// Features/Employees/SearchEmployees/SearchEmployeesQuery.cs
[QueryType]
public class SearchEmployeesQuery
{
    private readonly IAmazonDynamoDB _dynamoDb;
    
    public async Task<IEnumerable<Employee>> SearchEmployees(SearchEmployeeInput input)
    {
        // All search logic here
    }
}

// Input model in the SAME file
public class SearchEmployeeInput
{
    public string? Department { get; set; }
    public decimal? MinSalary { get; set; }
}

// Validator in the SAME file (if needed)
public class SearchEmployeeInputValidator : AbstractValidator<SearchEmployeeInput>
{
    public SearchEmployeeInputValidator()
    {
        // Validation rules here
    }
}
```

Then just add to Program.cs:
```csharp
.AddTypeExtension<SearchEmployeesQuery>()
```

## Key Principle: Everything in ONE File

Each feature file contains:
- The GraphQL query/mutation class
- Input models specific to that feature
- Validators for those inputs
- Any output/payload models (if using result types)
- All business logic for the feature

## Migration from Traditional Architecture

We migrated from:
- Separate Types/Query.cs and Types/Mutation.cs
- Types/Inputs/ folder with input models
- Validators/ folder with validation classes
- IEmployeeRepository and DynamoDbEmployeeRepository

To:
- Single file per feature containing EVERYTHING
- Direct DynamoDB access within each feature
- No separation between technical concerns

This true VSA approach means:
- Opening ONE file shows you the entire feature
- No jumping between folders to understand functionality
- Changes to a feature only affect ONE file
- New developers can understand features instantly