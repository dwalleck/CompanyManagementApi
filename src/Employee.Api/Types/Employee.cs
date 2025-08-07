using System;
using System.ComponentModel.DataAnnotations;

namespace Employee.Api.Types;

public class Employee
{
    [Key]
    public string EmployeeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Department { get; set; } = string.Empty;
    
    public decimal Salary { get; set; }
    
    public DateTime HireDate { get; set; }
    
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
