namespace Zeta.Core;

/// <summary>
/// Provides execution context for the current validation operation, including the current path, services, and cancellation token.
/// </summary>
public sealed class ValidationExecutionContext
{
    private readonly IServiceProvider? _services;

    /// <summary>
    /// The dot-notation path to the current value being validated (e.g., "user.address.street").
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services are not available in this validation context.");

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
    public ValidationExecutionContext(
        IServiceProvider? services = null,
        CancellationToken cancellationToken = default,
        TimeProvider? timeProvider = null) : this("", services, cancellationToken, timeProvider)
    {
    }

    internal ValidationExecutionContext(
        string path = "",
        IServiceProvider? services = null,
        CancellationToken cancellationToken = default,
        TimeProvider? timeProvider = null)
    {
        Path = path ?? "";
        _services = services;
        CancellationToken = cancellationToken;
        TimeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public ValidationExecutionContext Push(string segment)
    {
        var newPath = string.IsNullOrEmpty(Path) ? segment : $"{Path}.{segment}";
        return new ValidationExecutionContext(newPath, _services, CancellationToken, TimeProvider);
    }

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public ValidationExecutionContext PushIndex(int index)
    {
        var newPath = string.IsNullOrEmpty(Path) ? $"[{index}]" : $"{Path}[{index}]";
        return new ValidationExecutionContext(newPath, _services, CancellationToken, TimeProvider);
    }

    /// <summary>
    /// Gets default empty context (cached instance).
    /// </summary>
    public static ValidationExecutionContext Empty { get; } = new(null, CancellationToken.None, TimeProvider.System);
}