namespace Zeta;

/// <summary>
/// Provides a strongly-typed context for validation, including shared async data and execution details.
/// </summary>
/// <typeparam name="TData">The type of the shared data context.</typeparam>
public record ValidationContext<TData> : ValidationContext
{
    /// <summary>
    /// The shared data context (e.g., loaded from database).
    /// </summary>
    public TData Data { get; }

    public ValidationContext(
        TData data,
        IServiceProvider? services = null,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default) : base(services, timeProvider, cancellationToken)
    {
        Data = data;
    }

    private ValidationContext(
        string path,
        TData data,
        IServiceProvider? services = null,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default) : base(path, services, timeProvider, cancellationToken)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public new ValidationContext<TData> Push(string segment)
    {
        var newPath = string.IsNullOrEmpty(Path) ? segment : $"{Path}.{segment}";
        return new ValidationContext<TData>(newPath, Data, ServiceProvider, TimeProvider, CancellationToken);
    }

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public new ValidationContext<TData> PushIndex(int index)
    {
        var newPath = string.IsNullOrEmpty(Path) ? $"[{index}]" : $"{Path}[{index}]";
        return new ValidationContext<TData>(newPath, Data, ServiceProvider, TimeProvider, CancellationToken);
    }
}

/// <summary>
/// Provides a strongly-typed context for validation, including shared async data and execution details.
/// </summary>
public record ValidationContext
{
    protected IServiceProvider? ServiceProvider { get; init; }

    /// <summary>
    /// The dot-notation path to the current value being validated (e.g., "user.address.street").
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services => ServiceProvider ?? throw new InvalidOperationException("Services are not available in this validation context.");

    /// <summary>
    /// The cancellation token for async operations.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// The time provider for time-based validations. Defaults to <see cref="TimeProvider.System"/>.
    /// </summary>
    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// Creates a new validation execution context.
    /// </summary>
    public ValidationContext(
        IServiceProvider? services = null,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default) : this("", services, timeProvider, cancellationToken)
    {
    }

    protected ValidationContext(
        string path = "",
        IServiceProvider? services = null,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default)
    {
        Path = path ?? "";
        ServiceProvider = services;
        CancellationToken = cancellationToken;
        TimeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public ValidationContext Push(string segment)
    {
        var newPath = string.IsNullOrEmpty(Path) ? segment : $"{Path}.{segment}";
        return new ValidationContext(newPath, ServiceProvider, TimeProvider, CancellationToken);
    }

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public ValidationContext PushIndex(int index)
    {
        var newPath = string.IsNullOrEmpty(Path) ? $"[{index}]" : $"{Path}[{index}]";
        return new ValidationContext(newPath, ServiceProvider, TimeProvider, CancellationToken);
    }

    /// <summary>
    /// Gets default empty context (cached instance).
    /// </summary>
    public static ValidationContext Empty { get; } = new(null, TimeProvider.System, CancellationToken.None);
}