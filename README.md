# Company Management API

A modern Employee API built with GraphQL, AWS DynamoDB, and Vertical Slice Architecture.

## Features

- **GraphQL API** using Hot Chocolate
- **AWS DynamoDB** for data persistence
- **Vertical Slice Architecture** for better code organization
- **FluentValidation** for input validation
- **AWS Lambda** ready (both ZIP and container deployment)
- **.NET Aspire** integration for local development
- **OpenTelemetry** instrumentation

## Architecture

This project follows Vertical Slice Architecture (VSA) principles:

```
src/Employee.Api/
├── Features/
│   └── Employees/
│       ├── GetEmployeeQuery.cs
│       ├── GetAllEmployeesQuery.cs
│       ├── AddEmployeeCommand.cs
│       ├── UpdateEmployeeCommand.cs
│       └── DeleteEmployeeCommand.cs
```

Each feature file contains:
- GraphQL query/mutation definition
- Input models
- Validation rules
- Business logic
- Data access

## Prerequisites

- .NET 8.0 SDK
- Docker (for local DynamoDB and container deployment)
- AWS CLI (for Lambda deployment)

## Running Locally

### Direct Run
```bash
cd src/Employee.Api
dotnet run
```

The API will be available at `http://localhost:5000/graphql`

### With .NET Aspire
```bash
cd src/Employee.Api.AppHost
dotnet run
```

This will start:
- Local DynamoDB instance
- Employee API connected to local DynamoDB

## GraphQL Operations

### Queries

```graphql
query GetEmployee {
  employee(employeeId: "12345") {
    employeeId
    name
    department
    salary
    hireDate
  }
}

query GetAllEmployees {
  employees {
    employeeId
    name
    department
  }
}
```

### Mutations

```graphql
mutation AddEmployee {
  addEmployee(input: {
    name: "John Doe"
    department: "Engineering"
    salary: 75000
  }) {
    employeeId
    name
  }
}

mutation UpdateEmployee {
  updateEmployee(input: {
    employeeId: "12345"
    salary: 80000
  }) {
    employeeId
    salary
  }
}

mutation DeleteEmployee {
  deleteEmployee(employeeId: "12345")
}
```

## Deployment

### AWS Lambda (ZIP)
```bash
cd src/Employee.Api
dotnet lambda deploy-function
```

### AWS Lambda (Container)
```bash
cd src/Employee.Api
docker build -t employee-api .
# Push to ECR and deploy
```

## Configuration

### DynamoDB Table
- Table Name: `Employees` (configurable in appsettings.json)
- Partition Key: `EmployeeId` (String)

### Environment Variables
- `AWS_ENDPOINT_URL_DYNAMODB`: Set by Aspire for local development
- `AWS_LAMBDA_FUNCTION_NAME`: Automatically set in Lambda environment

## Contributing

This is a demo project showcasing modern .NET development practices with AWS services.