using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema that wraps a contextless schema and allows adding context-aware refinements.
/// Created via the <c>.WithContext&lt;TContext&gt;()</c> extension method.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TContext">The context type for context-aware refinements.</typeparam>
public class ContextPromotedSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly ISchema<T, object?> _inner;
    private readonly List<ISyncRule<T, TContext>> _syncRules = [];
    private readonly List<IAsyncRule<T, TContext>> _asyncRules = [];

    public ContextPromotedSchema(ISchema<T, object?> inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        // First run the inner (contextless) schema
        var innerContext = new ValidationContext<object?>(null, context.Execution);
        var innerResult = await _inner.ValidateAsync(value, innerContext);

        List<ValidationError>? errors = null;

        // Collect inner errors
        if (innerResult.IsFailure)
        {
            errors = [..innerResult.Errors];
        }

        // Execute context-aware sync rules
        foreach (var rule in _syncRules)
        {
            var error = rule.Validate(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        // Execute context-aware async rules
        foreach (var rule in _asyncRules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors == null ? Result.Success() : Result.Failure(errors);
    }

    /// <summary>
    /// Adds a synchronous context-aware refinement.
    /// </summary>
    /// <param name="predicate">A function that takes the value and context data, returning true if valid.</param>
    /// <param name="message">The error message if validation fails.</param>
    /// <param name="code">The error code (default: "custom_error").</param>
    public ContextPromotedSchema<T, TContext> Refine(
        Func<T, TContext, bool> predicate,
        string message,
        string code = "custom_error")
    {
        _syncRules.Add(new DelegateSyncRule<T, TContext>((val, ctx) =>
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
    public ContextPromotedSchema<T, TContext> Refine(
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
    public ContextPromotedSchema<T, TContext> RefineAsync(
        Func<T, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        _asyncRules.Add(new DelegateAsyncRule<T, TContext>(async (val, ctx) =>
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
    public ContextPromotedSchema<T, TContext> RefineAsync(
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
