namespace Employee.Api.Common;

public record Error(
    string Message,
    string Code = "",
    Dictionary<string, object>? Details = null)
{
    public Dictionary<string, object> Details { get; } = Details ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}