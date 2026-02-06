using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class ContextSchema<T, TContext> : ISchema<T, TContext>
{
    protected ContextRuleEngine<T, TContext> Rules { get; }

    /// <summary>
    /// Indicates whether this schema allows null values.
    /// When false, null values return a "required" error.
    /// When true, null values are accepted and inner validation is skipped.
    /// </summary>
    protected bool IsNullable;

    protected ContextSchema() : this(new ContextRuleEngine<T, TContext>()) { }

    protected ContextSchema(ContextRuleEngine<T, TContext> rules)
    {
        Rules = rules;
    }

    /// <summary>
    /// Marks this schema as nullable, allowing null values to pass validation.
    /// </summary>
    public void MarkAsNullable()
    {
        IsNullable = true;
    }

    public virtual async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        // Check for null values
        if (value is null)
        {
            return IsNullable
                ? Result.Success()
                : Result.Failure(new[] { new ValidationError(context.Path, "required", "This field is required.") });
        }

        var errors = await Rules.ExecuteAsync(value, context);

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    protected void Use(IValidationRule<T, TContext> rule)
    {
        Rules.Add(rule);
    }
}