using Zeta.Core;

namespace Zeta;

/// <summary>
/// Provides a strongly-typed context for validation, including shared async data and execution details.
/// </summary>
/// <typeparam name="TData">The type of the shared data context.</typeparam>
public readonly struct ValidationContext<TData>
{
    /// <summary>
    /// The shared data context (e.g., loaded from database).
    /// </summary>
    public TData Data { get; }

    /// <summary>
    /// The execution context (path, services, cancellation token).
    /// </summary>
    public ValidationExecutionContext Execution { get; }

    public ValidationContext(TData data, ValidationExecutionContext execution)
    {
        Data = data;
        Execution = execution;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public ValidationContext<TData> Push(string segment)
    {
        return new ValidationContext<TData>(Data, Execution.Push(segment));
    }
}
