using HotChocolate;
using Employee.Api.Exceptions;

namespace Employee.Api.ErrorHandling;

public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        return error.Exception switch
        {
            EmployeeNotFoundException notFoundEx => ErrorBuilder.FromError(error)
                .SetMessage(notFoundEx.Message)
                .SetCode("EMPLOYEE_NOT_FOUND")
                .Build(),
                
            EmployeeAlreadyExistsException existsEx => ErrorBuilder.FromError(error)
                .SetMessage(existsEx.Message)
                .SetCode("EMPLOYEE_ALREADY_EXISTS")
                .Build(),
                
            ValidationException validationEx => ErrorBuilder.FromError(error)
                .SetMessage(validationEx.Message)
                .SetCode("VALIDATION_ERROR")
                .SetExtension("errors", validationEx.Errors)
                .Build(),
                
            _ => HandleUnexpectedError(error),
        };
    }
    
    private IError HandleUnexpectedError(IError error)
    {
        _logger.LogError(error.Exception, "Unexpected GraphQL error");
        
        return ErrorBuilder.FromError(error)
            .SetMessage("An unexpected error occurred")
            .SetCode("INTERNAL_ERROR")
            .Build();
    }
}