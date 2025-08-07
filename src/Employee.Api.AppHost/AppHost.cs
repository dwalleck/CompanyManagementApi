var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL for development
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var database = postgres.AddDatabase("employees");

// Add the Employee API project
var api = builder.AddProject<Projects.Employee_Api>("employee-api")
    .WithReference(database);

builder.Build().Run();
