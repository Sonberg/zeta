namespace Zeta;

/// <summary>
/// Represents the result of a validation operation.
/// Either contains a valid value or a collection of validation errors.
/// </summary>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly IReadOnlyList<ValidationError>? _errors;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the validated value. Throws if validation failed.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result. Check IsSuccess first.");

    /// <summary>
    /// Gets the validation errors. Empty if validation succeeded.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _errors ?? [];

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _errors = null;
    }

    private Result(IReadOnlyList<ValidationError> errors)
    {
        IsSuccess = false;
        _value = default;
        _errors = errors;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the given errors.
    /// </summary>
    public static Result<T> Failure(params ValidationError[] errors) => new(errors);

    /// <summary>
    /// Creates a failed result with the given errors.
    /// </summary>
    public static Result<T> Failure(IReadOnlyList<ValidationError> errors) => new(errors);

    /// <summary>
    /// Maps the value if successful, preserving errors if failed.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> map)
    {
        return IsSuccess
            ? Result<TNew>.Success(map(_value!))
            : Result<TNew>.Failure(_errors!);
    }

    /// <summary>
    /// Chains another validation if successful, preserving errors if failed.
    /// </summary>
    public async Task<Result<TNew>> Then<TNew>(Func<T, Task<Result<TNew>>> bind)
    {
        return IsSuccess
            ? await bind(_value!)
            : Result<TNew>.Failure(_errors!);
    }

    /// <summary>
    /// Chains another validation synchronously if successful.
    /// </summary>
    public Result<TNew> Then<TNew>(Func<T, Result<TNew>> bind)
    {
        return IsSuccess
            ? bind(_value!)
            : Result<TNew>.Failure(_errors!);
    }

    /// <summary>
    /// Pattern matches on the result.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> success, Func<IReadOnlyList<ValidationError>, TResult> failure)
    {
        return IsSuccess ? success(_value!) : failure(_errors!);
    }

    /// <summary>
    /// Pattern matches on the result with actions.
    /// </summary>
    public void Match(Action<T> success, Action<IReadOnlyList<ValidationError>> failure)
    {
        if (IsSuccess)
            success(_value!);
        else
            failure(_errors!);
    }

    /// <summary>
    /// Gets the value or returns the fallback if validation failed.
    /// </summary>
    public T GetOrDefault(T fallback) => IsSuccess ? _value! : fallback;

    /// <summary>
    /// Gets the value or throws with a descriptive message if validation failed.
    /// </summary>
    public T GetOrThrow()
    {
        if (IsSuccess) return _value!;

        var messages = string.Join("; ", _errors!.Select(e =>
            string.IsNullOrEmpty(e.Path) ? e.Message : $"{e.Path}: {e.Message}"));
        throw new ValidationException(messages, _errors!);
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Exception thrown when GetOrThrow is called on a failed result.
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(string message, IReadOnlyList<ValidationError> errors) : base(message)
    {
        Errors = errors;
    }
}

/// <summary>
/// Extension methods for Result.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Combines multiple results, returning all errors if any failed.
    /// </summary>
    public static Result<T[]> Combine<T>(this IEnumerable<Result<T>> results)
    {
        var list = results.ToList();
        var errors = list.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();

        return errors.Count > 0
            ? Result<T[]>.Failure(errors)
            : Result<T[]>.Success(list.Select(r => r.Value).ToArray());
    }

    /// <summary>
    /// Awaits a task and chains another async operation.
    /// </summary>
    public static async Task<Result<TNew>> Then<T, TNew>(
        this Task<Result<T>> task,
        Func<T, Task<Result<TNew>>> bind)
    {
        var result = await task;
        return await result.Then(bind);
    }

    /// <summary>
    /// Awaits a task and maps the result.
    /// </summary>
    public static async Task<Result<TNew>> Map<T, TNew>(
        this Task<Result<T>> task,
        Func<T, TNew> map)
    {
        var result = await task;
        return result.Map(map);
    }
}
