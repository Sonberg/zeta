using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Rules.String;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// String validators. Work on both contextless (<see cref="StringContextlessSchema"/>)
/// and context-aware (<see cref="StringContextSchema{TContext}"/>) string schemas.
/// </summary>
public static class StringSchemaExtensions
{
    /// <summary>Requires the string to be at least <paramref name="min"/> characters long.</summary>
    public static TSelf MinLength<TSelf>(this IValueSchema<string, TSelf> schema, int min, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new MinLengthRule(min, message));

    /// <summary>Requires the string to be at most <paramref name="max"/> characters long.</summary>
    public static TSelf MaxLength<TSelf>(this IValueSchema<string, TSelf> schema, int max, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new MaxLengthRule(max, message));

    /// <summary>Requires the string to be exactly <paramref name="exact"/> characters long.</summary>
    public static TSelf Length<TSelf>(this IValueSchema<string, TSelf> schema, int exact, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new LengthRule(exact, message));

    /// <summary>Requires the string to be non-empty and not whitespace-only.</summary>
    public static TSelf NotEmpty<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new NotEmptyRule(message));

    /// <summary>Requires the string to be a valid email address.</summary>
    public static TSelf Email<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new EmailRule(message));

    /// <summary>Requires the string to be a valid UUID/GUID.</summary>
    public static TSelf Uuid<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new UuidRule(message));

    /// <summary>Requires the string to be a valid URL.</summary>
    public static TSelf Url<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new UrlRule(message));

    /// <summary>Requires the string to be a valid URI of the given <paramref name="kind"/>.</summary>
    public static TSelf Uri<TSelf>(this IValueSchema<string, TSelf> schema, UriKind kind = UriKind.Absolute, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new UriRule(kind, message));

    /// <summary>Requires the string to contain only letters and digits.</summary>
    public static TSelf Alphanumeric<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new AlphanumericRule(message));

    /// <summary>Requires the string to start with <paramref name="prefix"/>.</summary>
    public static TSelf StartsWith<TSelf>(this IValueSchema<string, TSelf> schema, string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new StartsWithRule(prefix, comparison, message));

    /// <summary>Requires the string to end with <paramref name="suffix"/>.</summary>
    public static TSelf EndsWith<TSelf>(this IValueSchema<string, TSelf> schema, string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new EndsWithRule(suffix, comparison, message));

    /// <summary>Requires the string to contain <paramref name="substring"/>.</summary>
    public static TSelf Contains<TSelf>(this IValueSchema<string, TSelf> schema, string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new ContainsRule(substring, comparison, message));

    /// <summary>Requires the string to match the given regular expression <paramref name="pattern"/>.</summary>
    public static TSelf Regex<TSelf>(this IValueSchema<string, TSelf> schema, string pattern, string? message = null, string code = "regex")
        where TSelf : IValueSchema<string, TSelf>
    {
        var compiled = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        return schema.AppendRule(new RegexRule(compiled, message, code));
    }
}
