var builder = DistributedApplication.CreateBuilder(args);

// Add DynamoDB local for development
var dynamoDb = builder.AddAWSDynamoDBLocal("dynamodb");

// Add the Employee API project
var api = builder.AddProject<Projects.Employee_Api>("employee-api")
    .WithReference(dynamoDb);

builder.Build().Run();
