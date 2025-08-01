# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build

```bash
dotnet build
```

### Run Locally

```bash
# Direct run (requires local DynamoDB setup)
cd src/Employee.Api
dotnet run

# With .NET Aspire (automatically sets up local DynamoDB)
cd src/Employee.Api.AppHost
dotnet run
```

### GraphQL Commands

Hot Chocolate includes command-line tools for GraphQL schema operations:

```bash
# Export GraphQL schema
dotnet run -- schema export --output schema.graphql

# Watch mode for schema changes
dotnet run -- schema watch
```

### AWS Lambda Deployment

```bash
# Deploy as ZIP package
cd src/Employee.Api
dotnet lambda deploy-function

# Build Docker container for Lambda
docker build -t employee-api -f src/Employee.Api/Dockerfile .
```

## Architecture Overview

This project uses **Vertical Slice Architecture (VSA)** where each feature is self-contained in a single file:

### Feature Structure

Each feature in `src/Employee.Api/Features/Employees/` contains:

- GraphQL query/mutation definition (marked with `[QueryType]` or `[MutationType]`)
- Input models specific to that feature
- FluentValidation validators
- Business logic and DynamoDB operations
- All in one file (e.g., `AddEmployeeCommand.cs`)

### Key Architectural Decisions

1. **GraphQL Type Registration**: Features are registered as type extensions in Program.cs:

   ```csharp
   .AddQueryType()
       .AddTypeExtension<GetEmployeeQuery>()
   .AddMutationType()
       .AddTypeExtension<AddEmployeeCommand>()
   ```

2. **DynamoDB Integration**:
   - Uses AWS SDK's DynamoDBContext for data access
   - Table name configured via `DynamoDbConfiguration`
   - Automatic table creation via `DynamoDbInitializer` hosted service

3. **Validation Pattern**:
   - Each command includes its own `AbstractValidator<TInput>` class
   - Validators are auto-registered via `AddValidatorsFromAssemblyContaining<Program>()`

4. **Environment Detection**:
   - Lambda environment: Checks for `AWS_LAMBDA_FUNCTION_NAME`
   - Aspire environment: Checks for `AWS_ENDPOINT_URL_DYNAMODB`
   - Adjusts AWS credentials and endpoints accordingly

5. **Error Handling**:
   - Custom `GraphQLErrorFilter` for consistent error responses
   - Business exceptions (e.g., `EmployeeNotFoundException`) are re-thrown
   - Validation errors include field-specific details

## Important Patterns

- **OneOf Package**: Available but not currently used - could be applied for discriminated union result types
- **OpenTelemetry**: Integrated for distributed tracing and logging
- **AWS Lambda Powertools**: Used for structured logging in Lambda environment
- **Hot Chocolate Instrumentation**: GraphQL-specific telemetry

## DynamoDB Schema

- Table: `Employees`
- Partition Key: `EmployeeId` (String)
- Attributes mapped via `[DynamoDBProperty]` attributes on the Employee type
