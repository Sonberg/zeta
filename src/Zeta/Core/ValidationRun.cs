namespace Zeta;

/// <summary>
/// Provides a strongly-typed context for validation, including shared async data and execution details.
/// </summary>
/// <typeparam name="TData">The type of the shared data context.</typeparam>
public record ValidationRun<TData> : ValidationRun
{
    /// <summary>
    /// The shared data context (e.g., loaded from database).
    /// </summary>
    public TData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRun{TData}"/> class.
    /// </summary>
    /// <param name="data">The shared context data.</param>
    /// <param name="timeProvider">Optional time provider. Defaults to <see cref="TimeProvider.System"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="serviceProvider">Optional service provider for dependency injection.</param>
    /// <param name="pathFormattingOptions">Optional path formatting options.</param>
    public ValidationRun(
        TData data,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null,
        PathFormattingOptions? pathFormattingOptions = null)
        : base(null, timeProvider, cancellationToken, serviceProvider, pathFormattingOptions)
    {
        Data = data;
    }

    internal ValidationRun(
        ValidationPath pathSegments,
        TData data,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null,
        PathFormattingOptions? pathFormattingOptions = null)
        : base(pathSegments, timeProvider, cancellationToken, serviceProvider, pathFormattingOptions)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public new ValidationRun<TData> Push(string segment)
        => new(
            PathSegments.Append(PathSegment.Property(segment)),
            Data,
            TimeProvider,
            CancellationToken,
            ServiceProvider,
            PathFormattingOptions);

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public new ValidationRun<TData> PushIndex(int index)
        => new(
            PathSegments.Append(PathSegment.Index(index)),
            Data,
            TimeProvider,
            CancellationToken,
            ServiceProvider,
            PathFormattingOptions);

    /// <summary>
    /// Creates a new context with a dictionary key appended to the path using bracket notation.
    /// </summary>
    public new ValidationRun<TData> PushKey<TKey>(TKey key) where TKey : notnull
        => new(
            PathSegments.Append(PathSegment.DictionaryKey(key)),
            Data,
            TimeProvider,
            CancellationToken,
            ServiceProvider,
            PathFormattingOptions);
}

/// <summary>
/// Provides a strongly-typed context for validation, including shared async data and execution details.
/// </summary>
public record ValidationRun
{
    private readonly ValidationPath _pathSegments;
    private string? _path;

    /// <summary>
    /// The dot-notation path to the current value being validated (e.g., "user.address.street"; "$" at the root).
    /// Rendered lazily from structured path segments.
    /// </summary>
    public string Path => _path ??= _pathSegments.Render(PathFormattingOptions) is { Length: > 0 } rendered ? rendered : "$";

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
    /// Controls how path segments are rendered to a final path string.
    /// </summary>
    public PathFormattingOptions PathFormattingOptions { get; }

    /// <summary>
    /// Creates a new validation execution context.
    /// </summary>
    public ValidationRun(
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null,
        PathFormattingOptions? pathFormattingOptions = null)
        : this(null, timeProvider, cancellationToken, serviceProvider, pathFormattingOptions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRun"/> class with structured path segments.
    /// </summary>
    private protected ValidationRun(
        ValidationPath? pathSegments,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default,
        IServiceProvider? serviceProvider = null,
        PathFormattingOptions? pathFormattingOptions = null)
    {
        var formatting = pathFormattingOptions ?? PathFormattingOptions.Default;
        _pathSegments = pathSegments ?? ValidationPath.CreateRoot(formatting);
        CancellationToken = cancellationToken;
        TimeProvider = timeProvider ?? TimeProvider.System;
        ServiceProvider = serviceProvider;
        PathFormattingOptions = formatting;
    }

    /// <summary>
    /// Creates a new context with the given path segment appended.
    /// </summary>
    public ValidationRun Push(string segment)
        => new(
            _pathSegments.Append(PathSegment.Property(segment)),
            TimeProvider,
            CancellationToken,
            ServiceProvider,
            PathFormattingOptions);

    /// <summary>
    /// Creates a new context with an array index appended to the path.
    /// </summary>
    public ValidationRun PushIndex(int index)
        => new(
            _pathSegments.Append(PathSegment.Index(index)),
            TimeProvider,
            CancellationToken,
            ServiceProvider,
            PathFormattingOptions);

    /// <summary>
    /// Creates a new context with a dictionary key appended to the path using bracket notation.
    /// </summary>
    public ValidationRun PushKey<TKey>(TKey key) where TKey : notnull
        => new(
            _pathSegments.Append(PathSegment.DictionaryKey(key)),
            TimeProvider,
            CancellationToken,
            ServiceProvider,
            PathFormattingOptions);

    /// <summary>
    /// Gets default empty context (cached instance).
    /// </summary>
    public static ValidationRun Empty { get; } =
        new(ValidationPath.Root, TimeProvider.System, CancellationToken.None, null, PathFormattingOptions.Default);
}
