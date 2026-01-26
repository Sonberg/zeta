using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Schemas;
using Zeta.Validation;

namespace Zeta;

public static class SchemaExtensions
{
    /// <summary>
    /// Validates a value using a context-aware schema with context data.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateAsync<T, TContext>(this ISchema<T, TContext> schema, T value, TContext data)
    {
        var result = await schema.ValidateAsync(value, new ValidationContext<TContext>(data, ValidationExecutionContext.Empty));

        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Errors);
    }

    // ==================== ContextPromotedSchema<string> Validation Extensions ====================

    /// <summary>
    /// Validates that the string has at least <paramref name="min"/> characters.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> MinLength<TContext>(
        this ContextPromotedSchema<string, TContext> schema, int min, string? message = null)
    {
        return schema.Refine(
            val => val.Length >= min,
            message ?? $"Must be at least {min} characters long",
            "min_length");
    }

    /// <summary>
    /// Validates that the string has at most <paramref name="max"/> characters.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> MaxLength<TContext>(
        this ContextPromotedSchema<string, TContext> schema, int max, string? message = null)
    {
        return schema.Refine(
            val => val.Length <= max,
            message ?? $"Must be at most {max} characters long",
            "max_length");
    }

    /// <summary>
    /// Validates that the string has exactly <paramref name="exact"/> characters.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Length<TContext>(
        this ContextPromotedSchema<string, TContext> schema, int exact, string? message = null)
    {
        return schema.Refine(
            val => val.Length == exact,
            message ?? $"Must be exactly {exact} characters long",
            "length");
    }

    /// <summary>
    /// Validates that the string is not empty or whitespace.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> NotEmpty<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => !string.IsNullOrWhiteSpace(val),
            message ?? "Value cannot be empty",
            "required");
    }

    /// <summary>
    /// Validates that the string is a valid email format.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Email<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => EmailRegex.IsMatch(val),
            message ?? "Invalid email format",
            "email");
    }

    /// <summary>
    /// Validates that the string is a valid UUID/GUID format.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Uuid<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => Guid.TryParse(val, out _),
            message ?? "Invalid UUID format",
            "uuid");
    }

    /// <summary>
    /// Validates that the string is a valid HTTP/HTTPS URL.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Url<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => System.Uri.TryCreate(val, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == System.Uri.UriSchemeHttp || uri.Scheme == System.Uri.UriSchemeHttps),
            message ?? "Invalid URL format",
            "url");
    }

    /// <summary>
    /// Validates that the string is a valid URI.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Uri<TContext>(
        this ContextPromotedSchema<string, TContext> schema, UriKind kind = UriKind.Absolute, string? message = null)
    {
        return schema.Refine(
            val => System.Uri.TryCreate(val, kind, out _),
            message ?? "Invalid URI format",
            "uri");
    }

    /// <summary>
    /// Validates that the string is a valid URI. Alias for Uri().
    /// </summary>
    [Obsolete("Use Uri() instead")]
    public static ContextPromotedSchema<string, TContext> ValidUri<TContext>(
        this ContextPromotedSchema<string, TContext> schema, UriKind kind = UriKind.Absolute, string? message = null)
        => Uri(schema, kind, message);

    /// <summary>
    /// Validates that the string contains only letters and numbers.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Alphanumeric<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => val.All(char.IsLetterOrDigit),
            message ?? "Must contain only letters and numbers",
            "alphanumeric");
    }

    /// <summary>
    /// Validates that the string starts with the specified prefix.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> StartsWith<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string prefix,
        StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        return schema.Refine(
            val => val.StartsWith(prefix, comparison),
            message ?? $"Must start with '{prefix}'",
            "starts_with");
    }

    /// <summary>
    /// Validates that the string ends with the specified suffix.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> EndsWith<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string suffix,
        StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        return schema.Refine(
            val => val.EndsWith(suffix, comparison),
            message ?? $"Must end with '{suffix}'",
            "ends_with");
    }

    /// <summary>
    /// Validates that the string contains the specified substring.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Contains<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string substring,
        StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        return schema.Refine(
            val => val.IndexOf(substring, comparison) >= 0,
            message ?? $"Must contain '{substring}'",
            "contains");
    }

    /// <summary>
    /// Validates that the string matches the specified regex pattern.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> Regex<TContext>(
        this ContextPromotedSchema<string, TContext> schema, string pattern,
        string? message = null, string code = "regex")
    {
        var compiledRegex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        return schema.Refine(
            val => compiledRegex.IsMatch(val),
            message ?? $"Must match pattern {pattern}",
            code);
    }

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    // ==================== ContextPromotedSchema<int> Validation Extensions ====================

    /// <summary>
    /// Validates that the integer is at least <paramref name="min"/>.
    /// </summary>
    public static ContextPromotedSchema<int, TContext> Min<TContext>(
        this ContextPromotedSchema<int, TContext> schema, int min, string? message = null)
    {
        return schema.Refine(
            val => val >= min,
            message ?? $"Must be at least {min}",
            "min");
    }

    /// <summary>
    /// Validates that the integer is at most <paramref name="max"/>.
    /// </summary>
    public static ContextPromotedSchema<int, TContext> Max<TContext>(
        this ContextPromotedSchema<int, TContext> schema, int max, string? message = null)
    {
        return schema.Refine(
            val => val <= max,
            message ?? $"Must be at most {max}",
            "max");
    }

    // ==================== ContextPromotedSchema<double> Validation Extensions ====================

    /// <summary>
    /// Validates that the double is at least <paramref name="min"/>.
    /// </summary>
    public static ContextPromotedSchema<double, TContext> Min<TContext>(
        this ContextPromotedSchema<double, TContext> schema, double min, string? message = null)
    {
        return schema.Refine(
            val => val >= min,
            message ?? $"Must be at least {min}",
            "min");
    }

    /// <summary>
    /// Validates that the double is at most <paramref name="max"/>.
    /// </summary>
    public static ContextPromotedSchema<double, TContext> Max<TContext>(
        this ContextPromotedSchema<double, TContext> schema, double max, string? message = null)
    {
        return schema.Refine(
            val => val <= max,
            message ?? $"Must be at most {max}",
            "max");
    }

    /// <summary>
    /// Validates that the double is positive (greater than zero).
    /// </summary>
    public static ContextPromotedSchema<double, TContext> Positive<TContext>(
        this ContextPromotedSchema<double, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => val > 0,
            message ?? "Must be positive",
            "positive");
    }

    /// <summary>
    /// Validates that the double is negative (less than zero).
    /// </summary>
    public static ContextPromotedSchema<double, TContext> Negative<TContext>(
        this ContextPromotedSchema<double, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => val < 0,
            message ?? "Must be negative",
            "negative");
    }

    /// <summary>
    /// Validates that the double is a finite number (not NaN or Infinity).
    /// </summary>
    public static ContextPromotedSchema<double, TContext> Finite<TContext>(
        this ContextPromotedSchema<double, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => !double.IsNaN(val) && !double.IsInfinity(val),
            message ?? "Must be a finite number",
            "finite");
    }

    // ==================== ContextPromotedSchema<decimal> Validation Extensions ====================

    /// <summary>
    /// Validates that the decimal is at least <paramref name="min"/>.
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> Min<TContext>(
        this ContextPromotedSchema<decimal, TContext> schema, decimal min, string? message = null)
    {
        return schema.Refine(
            val => val >= min,
            message ?? $"Must be at least {min}",
            "min");
    }

    /// <summary>
    /// Validates that the decimal is at most <paramref name="max"/>.
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> Max<TContext>(
        this ContextPromotedSchema<decimal, TContext> schema, decimal max, string? message = null)
    {
        return schema.Refine(
            val => val <= max,
            message ?? $"Must be at most {max}",
            "max");
    }

    /// <summary>
    /// Validates that the decimal is positive (greater than zero).
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> Positive<TContext>(
        this ContextPromotedSchema<decimal, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => val > 0,
            message ?? "Must be positive",
            "positive");
    }

    /// <summary>
    /// Validates that the decimal is negative (less than zero).
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> Negative<TContext>(
        this ContextPromotedSchema<decimal, TContext> schema, string? message = null)
    {
        return schema.Refine(
            val => val < 0,
            message ?? "Must be negative",
            "negative");
    }

    /// <summary>
    /// Validates that the decimal has at most <paramref name="maxDecimalPlaces"/> decimal places.
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> Precision<TContext>(
        this ContextPromotedSchema<decimal, TContext> schema, int maxDecimalPlaces, string? message = null)
    {
        return schema.Refine(
            val => GetDecimalPlaces(val) <= maxDecimalPlaces,
            message ?? $"Must have at most {maxDecimalPlaces} decimal places",
            "precision");
    }

    /// <summary>
    /// Validates that the decimal is a multiple of <paramref name="step"/>.
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> MultipleOf<TContext>(
        this ContextPromotedSchema<decimal, TContext> schema, decimal step, string? message = null)
    {
        return schema.Refine(
            val => val % step == 0,
            message ?? $"Must be a multiple of {step}",
            "multiple_of");
    }

    private static int GetDecimalPlaces(decimal value)
    {
        value = Math.Abs(value);
        value -= Math.Truncate(value);
        var places = 0;
        while (value > 0)
        {
            places++;
            value *= 10;
            value -= Math.Truncate(value);
        }
        return places;
    }

    // ==================== ContextPromotedSchema<TElement[]> Validation Extensions ====================

    /// <summary>
    /// Validates that the array has at least <paramref name="min"/> elements.
    /// </summary>
    public static ContextPromotedSchema<TElement[], TContext> MinLength<TElement, TContext>(
        this ContextPromotedSchema<TElement[], TContext> schema, int min, string? message = null)
    {
        return schema.Refine(
            val => val.Length >= min,
            message ?? $"Must have at least {min} items",
            "min_length");
    }

    /// <summary>
    /// Validates that the array has at most <paramref name="max"/> elements.
    /// </summary>
    public static ContextPromotedSchema<TElement[], TContext> MaxLength<TElement, TContext>(
        this ContextPromotedSchema<TElement[], TContext> schema, int max, string? message = null)
    {
        return schema.Refine(
            val => val.Length <= max,
            message ?? $"Must have at most {max} items",
            "max_length");
    }

    /// <summary>
    /// Validates that the array has exactly <paramref name="exact"/> elements.
    /// </summary>
    public static ContextPromotedSchema<TElement[], TContext> Length<TElement, TContext>(
        this ContextPromotedSchema<TElement[], TContext> schema, int exact, string? message = null)
    {
        return schema.Refine(
            val => val.Length == exact,
            message ?? $"Must have exactly {exact} items",
            "length");
    }

    /// <summary>
    /// Validates that the array is not empty.
    /// </summary>
    public static ContextPromotedSchema<TElement[], TContext> NotEmpty<TElement, TContext>(
        this ContextPromotedSchema<TElement[], TContext> schema, string? message = null)
    {
        return schema.MinLength<TElement, TContext>(1, message ?? "Must not be empty");
    }

    // ==================== ContextPromotedSchema<List<TElement>> Validation Extensions ====================

    /// <summary>
    /// Validates that the list has at least <paramref name="min"/> elements.
    /// </summary>
    public static ContextPromotedSchema<List<TElement>, TContext> MinLength<TElement, TContext>(
        this ContextPromotedSchema<List<TElement>, TContext> schema, int min, string? message = null)
    {
        return schema.Refine(
            val => val.Count >= min,
            message ?? $"Must have at least {min} items",
            "min_length");
    }

    /// <summary>
    /// Validates that the list has at most <paramref name="max"/> elements.
    /// </summary>
    public static ContextPromotedSchema<List<TElement>, TContext> MaxLength<TElement, TContext>(
        this ContextPromotedSchema<List<TElement>, TContext> schema, int max, string? message = null)
    {
        return schema.Refine(
            val => val.Count <= max,
            message ?? $"Must have at most {max} items",
            "max_length");
    }

    /// <summary>
    /// Validates that the list has exactly <paramref name="exact"/> elements.
    /// </summary>
    public static ContextPromotedSchema<List<TElement>, TContext> Length<TElement, TContext>(
        this ContextPromotedSchema<List<TElement>, TContext> schema, int exact, string? message = null)
    {
        return schema.Refine(
            val => val.Count == exact,
            message ?? $"Must have exactly {exact} items",
            "length");
    }

    /// <summary>
    /// Validates that the list is not empty.
    /// </summary>
    public static ContextPromotedSchema<List<TElement>, TContext> NotEmpty<TElement, TContext>(
        this ContextPromotedSchema<List<TElement>, TContext> schema, string? message = null)
    {
        return schema.MinLength<TElement, TContext>(1, message ?? "Must not be empty");
    }

    // ==================== String Schema ====================

    /// <summary>
    /// Creates a nullable version of this string schema that accepts null values.
    /// </summary>
    public static NullableSchema<string, TContext> Nullable<TContext>(this StringSchema<TContext> schema)
    {
        return new NullableSchema<string, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this string schema that accepts null values.
    /// </summary>
    public static NullableSchema<string> Nullable(this StringSchema schema)
    {
        return new NullableSchema<string>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<string, TContext> Optional<TContext>(this StringSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<string> Optional(this StringSchema schema)
        => schema.Nullable();

    // ==================== Int Schema ====================

    /// <summary>
    /// Creates a nullable version of this int schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<int, TContext> Nullable<TContext>(this IntSchema<TContext> schema)
    {
        return new NullableValueSchema<int, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this int schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<int> Nullable(this IntSchema schema)
    {
        return new NullableValueSchema<int>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<int, TContext> Optional<TContext>(this IntSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<int> Optional(this IntSchema schema)
        => schema.Nullable();

    // ==================== Double Schema ====================

    /// <summary>
    /// Creates a nullable version of this double schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<double, TContext> Nullable<TContext>(this DoubleSchema<TContext> schema)
    {
        return new NullableValueSchema<double, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this double schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<double> Nullable(this DoubleSchema schema)
    {
        return new NullableValueSchema<double>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<double, TContext> Optional<TContext>(this DoubleSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<double> Optional(this DoubleSchema schema)
        => schema.Nullable();

    // ==================== Decimal Schema ====================

    /// <summary>
    /// Creates a nullable version of this decimal schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<decimal, TContext> Nullable<TContext>(this DecimalSchema<TContext> schema)
    {
        return new NullableValueSchema<decimal, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this decimal schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<decimal> Nullable(this DecimalSchema schema)
    {
        return new NullableValueSchema<decimal>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<decimal, TContext> Optional<TContext>(this DecimalSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<decimal> Optional(this DecimalSchema schema)
        => schema.Nullable();

    // ==================== Object Schema ====================

    /// <summary>
    /// Creates a nullable version of this object schema that accepts null values.
    /// </summary>
    public static NullableSchema<T, TContext> Nullable<T, TContext>(this ObjectSchema<T, TContext> schema) where T : class
    {
        return new NullableSchema<T, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this object schema that accepts null values.
    /// </summary>
    public static NullableSchema<T> Nullable<T>(this ObjectSchema<T> schema) where T : class
    {
        return new NullableSchema<T>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<T, TContext> Optional<T, TContext>(this ObjectSchema<T, TContext> schema) where T : class
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<T> Optional<T>(this ObjectSchema<T> schema) where T : class
        => schema.Nullable();

    // ==================== Array Schema ====================

    /// <summary>
    /// Creates a nullable version of this array schema that accepts null values.
    /// </summary>
    public static NullableSchema<TElement[], TContext> Nullable<TElement, TContext>(this ArraySchema<TElement, TContext> schema)
    {
        return new NullableSchema<TElement[], TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this array schema that accepts null values.
    /// </summary>
    public static NullableSchema<TElement[]> Nullable<TElement>(this ArraySchema<TElement> schema)
    {
        return new NullableSchema<TElement[]>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<TElement[], TContext> Optional<TElement, TContext>(this ArraySchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<TElement[]> Optional<TElement>(this ArraySchema<TElement> schema)
        => schema.Nullable();

    // ==================== List Schema ====================

    /// <summary>
    /// Creates a nullable version of this list schema that accepts null values.
    /// </summary>
    public static NullableSchema<List<TElement>, TContext> Nullable<TElement, TContext>(this ListSchema<TElement, TContext> schema)
    {
        return new NullableSchema<List<TElement>, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this list schema that accepts null values.
    /// </summary>
    public static NullableSchema<List<TElement>> Nullable<TElement>(this ListSchema<TElement> schema)
    {
        return new NullableSchema<List<TElement>>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<List<TElement>, TContext> Optional<TElement, TContext>(this ListSchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<List<TElement>> Optional<TElement>(this ListSchema<TElement> schema)
        => schema.Nullable();

    // ==================== DateTime Schema ====================

    /// <summary>
    /// Creates a nullable version of this DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateTime, TContext> Nullable<TContext>(this DateTimeSchema<TContext> schema)
    {
        return new NullableValueSchema<DateTime, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateTime> Nullable(this DateTimeSchema schema)
    {
        return new NullableValueSchema<DateTime>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateTime, TContext> Optional<TContext>(this DateTimeSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateTime> Optional(this DateTimeSchema schema)
        => schema.Nullable();

#if !NETSTANDARD2_0
    // ==================== DateOnly Schema ====================

    /// <summary>
    /// Creates a nullable version of this DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateOnly, TContext> Nullable<TContext>(this DateOnlySchema<TContext> schema)
    {
        return new NullableValueSchema<DateOnly, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateOnly> Nullable(this DateOnlySchema schema)
    {
        return new NullableValueSchema<DateOnly>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateOnly, TContext> Optional<TContext>(this DateOnlySchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateOnly> Optional(this DateOnlySchema schema)
        => schema.Nullable();

    // ==================== TimeOnly Schema ====================

    /// <summary>
    /// Creates a nullable version of this TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<TimeOnly, TContext> Nullable<TContext>(this TimeOnlySchema<TContext> schema)
    {
        return new NullableValueSchema<TimeOnly, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<TimeOnly> Nullable(this TimeOnlySchema schema)
    {
        return new NullableValueSchema<TimeOnly>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<TimeOnly, TContext> Optional<TContext>(this TimeOnlySchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<TimeOnly> Optional(this TimeOnlySchema schema)
        => schema.Nullable();
#endif

    // ==================== Guid Schema ====================

    /// <summary>
    /// Creates a nullable version of this Guid schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<Guid, TContext> Nullable<TContext>(this GuidSchema<TContext> schema)
    {
        return new NullableValueSchema<Guid, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this Guid schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<Guid> Nullable(this GuidSchema schema)
    {
        return new NullableValueSchema<Guid>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<Guid, TContext> Optional<TContext>(this GuidSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<Guid> Optional(this GuidSchema schema)
        => schema.Nullable();

    // ==================== Bool Schema ====================

    /// <summary>
    /// Creates a nullable version of this bool schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<bool, TContext> Nullable<TContext>(this BoolSchema<TContext> schema)
    {
        return new NullableValueSchema<bool, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this bool schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<bool> Nullable(this BoolSchema schema)
    {
        return new NullableValueSchema<bool>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<bool, TContext> Optional<TContext>(this BoolSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<bool> Optional(this BoolSchema schema)
        => schema.Nullable();

    // ==================== Implicit Promotion Extensions ====================

    /// <summary>
    /// Adds a field with a context-aware schema, automatically promoting the object schema to context-aware.
    /// </summary>
    public static ContextPromotedObjectSchema<T, TContext> Field<T, TProperty, TContext>(
        this ObjectSchema<T> schema,
        System.Linq.Expressions.Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> fieldSchema) where T : class
    {
        return schema.WithContext<T, TContext>().Field(propertySelector, fieldSchema);
    }

    // ==================== WithContext Extensions ====================

    /// <summary>
    /// Promotes a contextless string schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<string, TContext> WithContext<TContext>(this StringSchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless int schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<int, TContext> WithContext<TContext>(this IntSchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless double schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<double, TContext> WithContext<TContext>(this DoubleSchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless decimal schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<decimal, TContext> WithContext<TContext>(this DecimalSchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless bool schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<bool, TContext> WithContext<TContext>(this BoolSchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless Guid schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<Guid, TContext> WithContext<TContext>(this GuidSchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless DateTime schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<DateTime, TContext> WithContext<TContext>(this DateTimeSchema schema)
        => new(schema);

#if !NETSTANDARD2_0
    /// <summary>
    /// Promotes a contextless DateOnly schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<DateOnly, TContext> WithContext<TContext>(this DateOnlySchema schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless TimeOnly schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<TimeOnly, TContext> WithContext<TContext>(this TimeOnlySchema schema)
        => new(schema);
#endif

    /// <summary>
    /// Promotes a contextless object schema to a context-aware schema, enabling context-aware refinements and field additions.
    /// </summary>
    public static ContextPromotedObjectSchema<T, TContext> WithContext<T, TContext>(this ObjectSchema<T> schema) where T : class
        => new(schema);

    /// <summary>
    /// Promotes a contextless array schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<TElement[], TContext> WithContext<TElement, TContext>(this ArraySchema<TElement> schema)
        => new(schema);

    /// <summary>
    /// Promotes a contextless list schema to a context-aware schema, enabling context-aware refinements.
    /// </summary>
    public static ContextPromotedSchema<List<TElement>, TContext> WithContext<TElement, TContext>(this ListSchema<TElement> schema)
        => new(schema);

    // ==================== ContextPromotedObjectSchema Nullable Extensions ====================

    /// <summary>
    /// Creates a nullable version of this context-promoted object schema that accepts null values.
    /// </summary>
    public static NullableSchema<T, TContext> Nullable<T, TContext>(this ContextPromotedObjectSchema<T, TContext> schema) where T : class
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted object schema that skips validation when null.
    /// </summary>
    public static NullableSchema<T, TContext> Optional<T, TContext>(this ContextPromotedObjectSchema<T, TContext> schema) where T : class
        => schema.Nullable();

    // ==================== ContextPromotedSchema Nullable Extensions (Reference Types) ====================

    /// <summary>
    /// Creates a nullable version of this context-promoted string schema that accepts null values.
    /// </summary>
    public static NullableSchema<string, TContext> Nullable<TContext>(this ContextPromotedSchema<string, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted string schema that skips validation when null.
    /// </summary>
    public static NullableSchema<string, TContext> Optional<TContext>(this ContextPromotedSchema<string, TContext> schema)
        => schema.Nullable();

    // ==================== ContextPromotedSchema Nullable Extensions (Value Types) ====================

    /// <summary>
    /// Creates a nullable version of this context-promoted int schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<int, TContext> Nullable<TContext>(this ContextPromotedSchema<int, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted int schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<int, TContext> Optional<TContext>(this ContextPromotedSchema<int, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted double schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<double, TContext> Nullable<TContext>(this ContextPromotedSchema<double, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted double schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<double, TContext> Optional<TContext>(this ContextPromotedSchema<double, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted decimal schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<decimal, TContext> Nullable<TContext>(this ContextPromotedSchema<decimal, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted decimal schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<decimal, TContext> Optional<TContext>(this ContextPromotedSchema<decimal, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted bool schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<bool, TContext> Nullable<TContext>(this ContextPromotedSchema<bool, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted bool schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<bool, TContext> Optional<TContext>(this ContextPromotedSchema<bool, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted Guid schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<Guid, TContext> Nullable<TContext>(this ContextPromotedSchema<Guid, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted Guid schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<Guid, TContext> Optional<TContext>(this ContextPromotedSchema<Guid, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateTime, TContext> Nullable<TContext>(this ContextPromotedSchema<DateTime, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted DateTime schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateTime, TContext> Optional<TContext>(this ContextPromotedSchema<DateTime, TContext> schema)
        => schema.Nullable();

#if !NETSTANDARD2_0
    /// <summary>
    /// Creates a nullable version of this context-promoted DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateOnly, TContext> Nullable<TContext>(this ContextPromotedSchema<DateOnly, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted DateOnly schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateOnly, TContext> Optional<TContext>(this ContextPromotedSchema<DateOnly, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<TimeOnly, TContext> Nullable<TContext>(this ContextPromotedSchema<TimeOnly, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted TimeOnly schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<TimeOnly, TContext> Optional<TContext>(this ContextPromotedSchema<TimeOnly, TContext> schema)
        => schema.Nullable();
#endif

    // ==================== ContextPromotedSchema Nullable Extensions (Arrays and Lists) ====================

    /// <summary>
    /// Creates a nullable version of this context-promoted array schema that accepts null values.
    /// </summary>
    public static NullableSchema<TElement[], TContext> Nullable<TElement, TContext>(this ContextPromotedSchema<TElement[], TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted array schema that skips validation when null.
    /// </summary>
    public static NullableSchema<TElement[], TContext> Optional<TElement, TContext>(this ContextPromotedSchema<TElement[], TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates a nullable version of this context-promoted list schema that accepts null values.
    /// </summary>
    public static NullableSchema<List<TElement>, TContext> Nullable<TElement, TContext>(this ContextPromotedSchema<List<TElement>, TContext> schema)
        => new(schema);

    /// <summary>
    /// Creates an optional version of this context-promoted list schema that skips validation when null.
    /// </summary>
    public static NullableSchema<List<TElement>, TContext> Optional<TElement, TContext>(this ContextPromotedSchema<List<TElement>, TContext> schema)
        => schema.Nullable();
}
