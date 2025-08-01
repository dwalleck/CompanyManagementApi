namespace Employee.Api.Configuration;

public class DynamoDbConfiguration
{
    public string TableName { get; set; } = "Employees";
}