# Local Development Guide

## Running with .NET Aspire

1. **Start the Aspire AppHost**:
   ```bash
   cd Employee.Api.AppHost
   dotnet run
   ```

   This will:
   - Start DynamoDB local on port 8000
   - Start the Employee API on port 5000
   - Open the Aspire dashboard

2. **The API will automatically**:
   - Connect to local DynamoDB at http://localhost:8000
   - Create the Employees table if it doesn't exist
   - Use dummy AWS credentials (no real AWS account needed)

3. **Access the API**:
   - GraphQL Playground: http://localhost:5000/graphql
   - Aspire Dashboard: http://localhost:15888 (or check console output)

## Running without Aspire

1. **Start DynamoDB Local** (using Docker):
   ```bash
   docker run -p 8000:8000 amazon/dynamodb-local
   ```

2. **Run the API**:
   ```bash
   cd src/Employee.Api
   dotnet run
   ```

3. **Create the table** (first time only):
   ```bash
   cd src/Employee.Api/Scripts
   ./create-local-table.sh
   ```

## Testing GraphQL Queries

Once running, try these queries in the GraphQL playground:

### Get all employees:
```graphql
query {
  employees {
    employeeId
    name
    department
    salary
  }
}
```

### Get a specific employee:
```graphql
query {
  employee(employeeId: "123") {
    __typename
    ... on Employee {
      employeeId
      name
      department
    }
    ... on EmployeeNotFoundError {
      message
    }
  }
}
```

### Add an employee:
```graphql
mutation {
  addEmployee(input: {
    name: "John Doe"
    department: "Engineering"
    salary: 75000
  }) {
    __typename
    ... on Employee {
      employeeId
      name
    }
    ... on ValidationError {
      message
      errors
    }
  }
}
```

## Configuration

The local development setup uses these settings from `appsettings.Development.json`:

```json
{
  "DynamoDb": {
    "TableName": "Employees",
    "UseLocalDynamoDb": true,
    "ServiceUrl": "http://localhost:8000"
  }
}
```

## Troubleshooting

1. **Port conflicts**: If port 8000 is in use, update the port in:
   - `appsettings.Development.json`
   - `Employee.Api.AppHost/Program.cs`

2. **Table not created**: The API should create the table automatically. If not:
   ```bash
   aws dynamodb create-table \
     --endpoint-url http://localhost:8000 \
     --table-name Employees \
     --attribute-definitions AttributeName=EmployeeId,AttributeType=S \
     --key-schema AttributeName=EmployeeId,KeyType=HASH \
     --billing-mode PAY_PER_REQUEST \
     --region us-east-1
   ```

3. **Connection issues**: Ensure DynamoDB local is running and accessible at the configured URL.