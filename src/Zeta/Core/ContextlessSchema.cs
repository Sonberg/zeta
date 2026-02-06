using Zeta.Rules;

namespace Zeta.Core;

public abstract class ContextlessSchema<T> : ISchema<T>
{
    protected ContextlessRuleEngine<T> Rules { get; }

    /// <summary>
    /// Indicates whether this schema allows null values.
    /// When false, null values return a "required" error.
    /// When true, null values are accepted and inner validation is skipped.
    /// </summary>
    protected bool IsNullable;

    protected ContextlessSchema() : this(new ContextlessRuleEngine<T>())
    {
    }

    protected ContextlessSchema(ContextlessRuleEngine<T> rules)
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

    public ValueTask<Result<T>> ValidateAsync(T value)
    {
        return ValidateAsync(value, ValidationContext.Empty);
    }

    public virtual async ValueTask<Result<T>> ValidateAsync(T value, ValidationContext context)
    {
        // Check for null values
        if (value is null)
        {
            return IsNullable
                ? Result<T>.Success(value)
                : Result<T>.Failure(new ValidationError(context.Path, "required", "This field is required."));
        }

        var errors = await Rules.ExecuteAsync(value, context);

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    protected void Use(IValidationRule<T> rule)
    {
        Rules.Add(rule);
    }
}