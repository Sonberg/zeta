using System.Linq.Expressions;
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-promoted object schema that wraps a contextless ObjectSchema and allows adding
/// context-aware refinements, fields, and conditional validations.
/// Created via the <c>.WithContext&lt;T, TContext&gt;()</c> extension method on ObjectSchema.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TContext">The context type for context-aware refinements.</typeparam>
public class ContextPromotedObjectSchema<T, TContext> : ISchema<T, TContext> where T : class
{
    private readonly ObjectSchema<T> _inner;
    private readonly List<IValidationRule<T, TContext>> _rules = [];
    private readonly List<IFieldValidator<T, TContext>> _fields = [];
    private readonly List<IConditionalBranch<T, TContext>> _conditionals = [];

    public ContextPromotedObjectSchema(ObjectSchema<T> inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        // First run the inner (contextless) schema
        var innerResult = await _inner.ValidateAsync(value, context.Execution);

        List<ValidationError>? errors = null;

        // Collect inner errors
        if (innerResult.IsFailure)
        {
            errors = [..innerResult.Errors];
        }

        // Validate context-aware fields
        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, context);
            if (fieldErrors.Count <= 0) continue;
            errors ??= [];
            errors.AddRange(fieldErrors);
        }

        // Execute context-aware conditionals
        foreach (var conditional in _conditionals)
        {
            var conditionalErrors = await conditional.ValidateAsync(value, context);
            if (conditionalErrors.Count <= 0) continue;
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        // Execute context-aware rules
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors == null ? Result.Success() : Result.Failure(errors);
    }

    /// <summary>
    /// Adds a field validation with a context-aware schema.
    /// </summary>
    public ContextPromotedObjectSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    /// <summary>
    /// Adds a field validation with a contextless schema (auto-adapted to context).
    /// </summary>
    public ContextPromotedObjectSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaAdapter<TProperty, TContext>(schema));
    }

    /// <summary>
    /// Conditionally validates fields based on a predicate.
    /// </summary>
    public ContextPromotedObjectSchema<T, TContext> When(
        Func<T, bool> condition,
        Action<ConditionalBuilder<T, TContext>> thenBranch,
        Action<ConditionalBuilder<T, TContext>>? elseBranch = null)
    {
        var thenBuilder = new ConditionalBuilder<T, TContext>();
        thenBranch(thenBuilder);

        ConditionalBuilder<T, TContext>? elseBuilder = null;
        if (elseBranch != null)
        {
            elseBuilder = new ConditionalBuilder<T, TContext>();
            elseBranch(elseBuilder);
        }

        _conditionals.Add(new ConditionalBranch<T, TContext>(condition, thenBuilder, elseBuilder));
        return this;
    }

    /// <summary>
    /// Conditionally validates fields based on a context-aware predicate.
    /// </summary>
    public ContextPromotedObjectSchema<T, TContext> When(
        Func<T, TContext, bool> condition,
        Action<ConditionalBuilder<T, TContext>> thenBranch,
        Action<ConditionalBuilder<T, TContext>>? elseBranch = null)
    {
        var thenBuilder = new ConditionalBuilder<T, TContext>();
        thenBranch(thenBuilder);

        ConditionalBuilder<T, TContext>? elseBuilder = null;
        if (elseBranch != null)
        {
            elseBuilder = new ConditionalBuilder<T, TContext>();
            elseBranch(elseBuilder);
        }

        _conditionals.Add(new ContextAwareConditionalBranch<T, TContext>(condition, thenBuilder, elseBuilder));
        return this;
    }

    /// <summary>
    /// Adds a synchronous context-aware refinement.
    /// </summary>
    /// <param name="predicate">A function that takes the value and context data, returning true if valid.</param>
    /// <param name="message">The error message if validation fails.</param>
    /// <param name="code">The error code (default: "custom_error").</param>
    public ContextPromotedObjectSchema<T, TContext> Refine(
        Func<T, TContext, bool> predicate,
        string message,
        string code = "custom_error")
    {
        _rules.Add(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Adds a synchronous refinement that doesn't use context.
    /// </summary>
    /// <param name="predicate">A function that takes the value, returning true if valid.</param>
    /// <param name="message">The error message if validation fails.</param>
    /// <param name="code">The error code (default: "custom_error").</param>
    public ContextPromotedObjectSchema<T, TContext> Refine(
        Func<T, bool> predicate,
        string message,
        string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    /// <summary>
    /// Adds an asynchronous context-aware refinement.
    /// </summary>
    /// <param name="predicate">An async function that takes the value, context data, and cancellation token, returning true if valid.</param>
    /// <param name="message">The error message if validation fails.</param>
    /// <param name="code">The error code (default: "custom_error").</param>
    public ContextPromotedObjectSchema<T, TContext> RefineAsync(
        Func<T, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        _rules.Add(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.Execution.CancellationToken)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Adds an asynchronous refinement that doesn't use context.
    /// </summary>
    /// <param name="predicate">An async function that takes the value and cancellation token, returning true if valid.</param>
    /// <param name="message">The error message if validation fails.</param>
    /// <param name="code">The error code (default: "custom_error").</param>
    public ContextPromotedObjectSchema<T, TContext> RefineAsync(
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
