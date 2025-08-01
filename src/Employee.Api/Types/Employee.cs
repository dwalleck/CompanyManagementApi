using System;
using Amazon.DynamoDBv2.DataModel;

namespace Employee.Api.Types;

[DynamoDBTable("Employees")]
public class Employee
{
    [DynamoDBHashKey]
    public string EmployeeId { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string Department { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public decimal Salary { get; set; }
    
    [DynamoDBProperty]
    public DateTime HireDate { get; set; }
    
    [DynamoDBProperty]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
