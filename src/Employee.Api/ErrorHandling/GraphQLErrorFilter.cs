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
        if (error.Exception is EmployeeNotFoundException notFoundEx)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(notFoundEx.Message)
                .SetCode("EMPLOYEE_NOT_FOUND")
                .Build();
        }

        if (error.Exception is EmployeeAlreadyExistsException existsEx)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(existsEx.Message)
                .SetCode("EMPLOYEE_ALREADY_EXISTS")
                .Build();
        }

        if (error.Exception is ValidationException validationEx)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(validationEx.Message)
                .SetCode("VALIDATION_ERROR")
                .SetExtension("errors", validationEx.Errors)
                .Build();
        }

        // Log unexpected errors
        _logger.LogError(error.Exception, "Unexpected GraphQL error");

        // For production, hide internal error details
        return ErrorBuilder.FromError(error)
            .SetMessage("An unexpected error occurred")
            .SetCode("INTERNAL_ERROR")
            .Build();
    }
}