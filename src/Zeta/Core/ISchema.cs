namespace Zeta;

/// <summary>
/// Defines a context-aware schema that can validate a value of type <typeparamref name="T"/>
/// with context <typeparamref name="TContext"/>.
/// </summary>
public interface ISchema<in T, TContext>
{
    internal bool AllowNull { get; }
    
    /// <summary>
    /// Validates the given value asynchronously with context.
    /// </summary>
    ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context);
}

/// <summary>
/// Defines a contextless schema that can validate a value of type <typeparamref name="T"/>.
/// This is completely separate from ISchema&lt;T, TContext&gt; - no inheritance relationship.
/// </summary>
public interface ISchema<T>
{
    /// <summary>
    /// Gets a value indicating whether null values are allowed by this schema.
    /// </summary>
    bool AllowNull { get; }
    
    /// <summary>
    /// Validates the given value asynchronously without context.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync(T? value, ValidationContext context);
}