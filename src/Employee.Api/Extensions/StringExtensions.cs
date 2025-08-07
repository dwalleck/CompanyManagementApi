using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Employee.Api.Extensions;

public static class StringExtensions
{
    private static readonly ConcurrentDictionary<string, string> _snakeCaseCache = new(StringComparer.Ordinal);
    private static readonly Regex _snakeCaseRegex = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));
    
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return _snakeCaseCache.GetOrAdd(input, static key => 
            _snakeCaseRegex.Replace(key, "_$1")
                          .Trim('_')
                          .ToLower());
    }
}