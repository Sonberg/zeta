namespace Zeta.Schemas;

/// <summary>
/// A schema for validating integer values.
/// </summary>
public sealed class IntSchema : ISchema<int>
{
    private readonly List<IRule<int>> _rules = new();

    /// <inheritdoc />
    public async Task<Result<int>> ValidateAsync(int value, ValidationContext? context = null)
    {
        context ??= ValidationContext.Empty;
        var errors = new List<ValidationError>();

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors.Add(error);
            }
        }

        return errors.Count == 0
            ? Result<int>.Success(value)
            : Result<int>.Failure(errors);
    }

    /// <summary>
    /// Adds a custom rule to the schema.
    /// </summary>
    public IntSchema Use(IRule<int> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds a rule that requires the integer to be at least a minimum value.
    /// </summary>
    public IntSchema Min(int min, string? message = null)
    {
        return Use(new DelegateRule<int>((val, ctx) =>
        {
            if (val >= min) return ValueTask.FromResult<ValidationError?>(null);

            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Path,
                "min_value",
                message ?? $"Must be at least {min}"));
        }));
    }

    /// <summary>
    /// Adds a rule that requires the integer to be at most a maximum value.
    /// </summary>
    public IntSchema Max(int max, string? message = null)
    {
        return Use(new DelegateRule<int>((val, ctx) =>
        {
            if (val <= max) return ValueTask.FromResult<ValidationError?>(null);

            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Path,
                "max_value",
                message ?? $"Must be at most {max}"));
        }));
    }

    /// <summary>
    /// Adds a rule that requires the integer to be positive (> 0).
    /// </summary>
    public IntSchema Positive(string? message = null)
    {
        return Min(1, message ?? "Must be positive");
    }

    /// <summary>
    /// Adds a rule that requires the integer to be negative (< 0).
    /// </summary>
    public IntSchema Negative(string? message = null)
    {
        return Max(-1, message ?? "Must be negative");
    }
}
