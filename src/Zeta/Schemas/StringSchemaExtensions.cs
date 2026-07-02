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
    public static TSelf MinLength<TSelf>(this IValueSchema<string, TSelf> schema, int min, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new MinLengthRule(min, message));

    public static TSelf MaxLength<TSelf>(this IValueSchema<string, TSelf> schema, int max, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new MaxLengthRule(max, message));

    public static TSelf Length<TSelf>(this IValueSchema<string, TSelf> schema, int exact, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new LengthRule(exact, message));

    public static TSelf NotEmpty<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new NotEmptyRule(message));

    public static TSelf Email<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new EmailRule(message));

    public static TSelf Uuid<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new UuidRule(message));

    public static TSelf Url<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new UrlRule(message));

    public static TSelf Uri<TSelf>(this IValueSchema<string, TSelf> schema, UriKind kind = UriKind.Absolute, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new UriRule(kind, message));

    public static TSelf Alphanumeric<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new AlphanumericRule(message));

    public static TSelf StartsWith<TSelf>(this IValueSchema<string, TSelf> schema, string prefix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new StartsWithRule(prefix, comparison, message));

    public static TSelf EndsWith<TSelf>(this IValueSchema<string, TSelf> schema, string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new EndsWithRule(suffix, comparison, message));

    public static TSelf Contains<TSelf>(this IValueSchema<string, TSelf> schema, string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
        where TSelf : IValueSchema<string, TSelf>
        => schema.AppendRule(new ContainsRule(substring, comparison, message));

    public static TSelf Regex<TSelf>(this IValueSchema<string, TSelf> schema, string pattern, string? message = null, string code = "regex")
        where TSelf : IValueSchema<string, TSelf>
    {
        var compiled = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        return schema.AppendRule(new RegexRule(compiled, message, code));
    }
}
