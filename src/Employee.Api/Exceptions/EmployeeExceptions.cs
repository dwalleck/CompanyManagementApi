namespace Employee.Api.Exceptions;

public class EmployeeNotFoundException : Exception
{
    public string EmployeeId { get; }

    public EmployeeNotFoundException(string employeeId) 
        : base($"Employee with ID '{employeeId}' was not found.")
    {
        EmployeeId = employeeId;
    }
}

public class EmployeeAlreadyExistsException : Exception
{
    public string EmployeeId { get; }

    public EmployeeAlreadyExistsException(string employeeId)
        : base($"Employee with ID '{employeeId}' already exists.")
    {
        EmployeeId = employeeId;
    }
}

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}