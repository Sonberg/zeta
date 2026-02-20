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

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationContext{TData}"/> class.
    /// </summary>
    /// <param name="data">The shared context data.</param>
    /// <param name="timeProvider">Optional time provider. Defaults to <see cref="TimeProvider.System"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="serviceProvider">Optional service provider for dependency injection.</param>
    public ValidationContext(
        TData data,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null)
        : base(ValidationPath.Root, timeProvider, cancellationToken, serviceProvider)
    {
        Data = data;
    }

    internal ValidationContext(
        ValidationPath pathSegments,
        TData data,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null)
        : base(pathSegments, timeProvider, cancellationToken, serviceProvider)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public new ValidationContext<TData> Push(string segment)
        => new(PathSegments.Append(PathSegment.Property(segment)), Data, TimeProvider, CancellationToken, ServiceProvider);

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public new ValidationContext<TData> PushIndex(int index)
        => new(PathSegments.Append(PathSegment.Index(index)), Data, TimeProvider, CancellationToken, ServiceProvider);

    /// <summary>
    /// Creates a new context with a dictionary key appended to the path using bracket notation.
    /// </summary>
    public new ValidationContext<TData> PushKey<TKey>(TKey key) where TKey : notnull
        => new(PathSegments.Append(PathSegment.DictionaryKey(key)), Data, TimeProvider, CancellationToken, ServiceProvider);
}

/// <summary>
/// Provides a strongly-typed context for validation, including shared async data and execution details.
/// </summary>
public record ValidationContext
{
    private readonly ValidationPath _pathSegments;

    /// <summary>
    /// The dot-notation path to the current value being validated (e.g., "user.address.street").
    /// Rendered lazily from structured path segments.
    /// </summary>
    public string Path => _pathSegments.Render();

    /// <summary>
    /// Internal structured path segments. Allows promotion to typed contexts without string rendering.
    /// </summary>
    internal ValidationPath PathSegments => _pathSegments;

    /// <summary>
    /// The cancellation token for async operations.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// The time provider for time-based validations. Defaults to <see cref="TimeProvider.System"/>.
    /// </summary>
    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// Optional service provider for dependency injection.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; }

    /// <summary>
    /// Creates a new validation execution context.
    /// </summary>
    public ValidationContext(
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null)
        : this(ValidationPath.Root, timeProvider, cancellationToken, serviceProvider)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationContext"/> class with structured path segments.
    /// </summary>
    private protected ValidationContext(
        ValidationPath? pathSegments,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null)
    {
        _pathSegments = pathSegments ?? ValidationPath.Root;
        CancellationToken = cancellationToken;
        TimeProvider = timeProvider ?? TimeProvider.System;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public ValidationContext Push(string segment)
        => new(_pathSegments.Append(PathSegment.Property(segment)), TimeProvider, CancellationToken, ServiceProvider);

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public ValidationContext PushIndex(int index)
        => new(_pathSegments.Append(PathSegment.Index(index)), TimeProvider, CancellationToken, ServiceProvider);

    /// <summary>
    /// Creates a new context with a dictionary key appended to the path using bracket notation.
    /// </summary>
    public ValidationContext PushKey<TKey>(TKey key) where TKey : notnull
        => new(_pathSegments.Append(PathSegment.DictionaryKey(key)), TimeProvider, CancellationToken, ServiceProvider);

    /// <summary>
    /// Gets default empty context (cached instance).
    /// </summary>
    public static ValidationContext Empty { get; } =
        new(ValidationPath.Root, TimeProvider.System, CancellationToken.None);
}
