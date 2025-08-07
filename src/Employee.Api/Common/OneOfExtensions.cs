using FluentValidation.Results;
using HotChocolate;
using HotChocolate.Resolvers;
using OneOf;

namespace Employee.Api.Common;

/// <summary>
/// Simple extension methods for using OneOf directly instead of a custom Result type
/// </summary>
public static class OneOfExtensions
{
    // Convert to GraphQL error
    public static IError ToGraphQLError(this Error error)
    {
        var builder = ErrorBuilder.New()
            .SetMessage(error.Message);

        if (!string.IsNullOrEmpty(error.Code))
        {
            builder.SetCode(error.Code);
            builder.SetExtension("code", error.Code);
        }

        foreach (var detail in error.Details)
        {
            builder.SetExtension(detail.Key, detail.Value);
        }

        return builder.Build();
    }

    // FluentValidation integration
    public static OneOf<T, Error> ToOneOf<T>(this ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
        {
            return value;
        }

        var groupedErrors = validationResult.Errors
            .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (object)g.Select(e => new
                {
                    message = e.ErrorMessage,
                    code = e.ErrorCode,
                    severity = e.Severity.ToString(),
                }).ToArray()
, StringComparer.OrdinalIgnoreCase);

        var details = new Dictionary<string, object>
(StringComparer.OrdinalIgnoreCase)
        {
            ["validationErrors"] = groupedErrors,
            ["errorCount"] = validationResult.Errors.Count,
        };

        var message = validationResult.Errors.Count == 1
            ? validationResult.Errors[0].ErrorMessage
            : $"{validationResult.Errors.Count} validation errors occurred";

        return new Error(message, "VALIDATION_ERROR", details);
    }

    // Resolver helper - report error or return value
    public static T? ResolveOrReport<T>(this OneOf<T, Error> result, IResolverContext context)
        where T : class
    {
        return result.Match(
            success => success,
            error =>
            {
                context.ReportError(error.ToGraphQLError());
                return null;
            }
        );
    }

    // For non-null types, throw if error
    public static T EnsureSuccess<T>(this OneOf<T, Error> result)
    {
        return result.Match(
            success => success,
            error => throw new GraphQLException(error.ToGraphQLError())
        );
    }

    // Async mapping
    public static async Task<OneOf<TNew, Error>> MapAsync<T, TNew>(
        this OneOf<T, Error> result,
        Func<T, Task<TNew>> mapper)
    {
        return await result.Match(
            async success => (OneOf<TNew, Error>)await mapper(success).ConfigureAwait(true),
            error => Task.FromResult<OneOf<TNew, Error>>(error)
        ).ConfigureAwait(true);
    }

    // Async binding
    public static async Task<OneOf<TNew, Error>> BindAsync<T, TNew>(
        this OneOf<T, Error> result,
        Func<T, Task<OneOf<TNew, Error>>> mapper)
    {
        return await result.Match(
            mapper,
            error => Task.FromResult<OneOf<TNew, Error>>(error)
        ).ConfigureAwait(true);
    }
}