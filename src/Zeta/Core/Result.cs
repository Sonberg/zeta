using System.Runtime.CompilerServices;

namespace Zeta;

/// <summary>
/// Represents the result of a validation operation.
/// Either contains a valid value or a collection of validation errors.
/// </summary>
public record Result
{
    private readonly IReadOnlyList<ValidationError>? _errors;

    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the validation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private static readonly Result SuccessValue = new();

    /// <summary>
    /// Gets the validation errors. Empty if validation succeeded.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _errors ?? [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class representing a successful validation.
    /// </summary>
    public Result()
    {
        IsSuccess = true;
        _errors = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class representing a failed validation.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    protected Result(IReadOnlyList<ValidationError> errors)
    {
        IsSuccess = false;
        _errors = errors;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result Success() => SuccessValue;

    /// <summary>
    /// Creates a failed result with the given errors.
    /// Duplicate errors are automatically removed.
    /// </summary>
    public static Result Failure(IReadOnlyList<ValidationError> errors) => new(errors);
}

/// <summary>
/// Represents the result of a validation operation.
/// Either contains a valid value or a collection of validation errors.
/// </summary>
public record Result<T> : Result
{
    private readonly T? _value;

    // Cache for reference type success results (object-based to work with any reference type)
    private static readonly ConditionalWeakTable<object, Result<T>>? _successCache =
        typeof(T).IsValueType ? null : new ConditionalWeakTable<object, Result<T>>();

    /// <summary>
    /// Gets the validated value. Throws if validation failed.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result. Check IsSuccess first.");

    private protected Result(T value)
    {
        _value = value;
    }

    private protected Result(IReadOnlyList<ValidationError> errors) : base(errors)
    {
        _value = default;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// For reference types, results are cached by reference identity.
    /// </summary>
    public static Result<T> Success(T value)
    {
        // For reference types, use caching to reduce allocations
        if (_successCache != null && value is not null)
        {
            return _successCache.GetValue(value, static v => new Result<T>((T)v));
        }

        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a failed result with the given errors.
    /// Duplicate errors are automatically removed.
    /// </summary>
    public static Result<T> Failure(params ValidationError[] errors) => new(errors);

    /// <summary>
    /// Creates a failed result with the given errors.
    /// Duplicate errors are automatically removed.
    /// </summary>
    public new static Result<T> Failure(IReadOnlyList<ValidationError> errors) => new(errors);

    /// <summary>
    /// Maps the value if successful, preserving errors if failed.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> map)
    {
        return IsSuccess
            ? Result<TNew>.Success(map(_value!))
            : Result<TNew>.Failure(Errors);
    }

    /// <summary>
    /// Chains another validation if successful, preserving errors if failed.
    /// </summary>
    public async Task<Result<TNew>> Then<TNew>(Func<T, Task<Result<TNew>>> bind)
    {
        return IsSuccess
            ? await bind(_value!)
            : Result<TNew>.Failure(Errors);
    }

    /// <summary>
    /// Chains another validation synchronously if successful.
    /// </summary>
    public Result<TNew> Then<TNew>(Func<T, Result<TNew>> bind)
    {
        return IsSuccess
            ? bind(_value!)
            : Result<TNew>.Failure(Errors);
    }

    /// <summary>
    /// Pattern matches on the result.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> success, Func<IReadOnlyList<ValidationError>, TResult> failure)
    {
        return IsSuccess ? success(_value!) : failure(Errors);
    }

    /// <summary>
    /// Pattern matches on the result with actions.
    /// </summary>
    public void Match(Action<T> success, Action<IReadOnlyList<ValidationError>> failure)
    {
        if (IsSuccess)
            success(_value!);
        else
            failure(Errors);
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

        var messages = string.Join("; ", Errors.Select(e =>
            string.IsNullOrEmpty(e.PathString) ? e.Message : $"{e.PathString}: {e.Message}"));
        throw new ValidationException(messages, Errors!);
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Represents the result of a context-aware validation operation.
/// Extends <see cref="Result{T}"/> with the resolved context data.
/// </summary>
public sealed record Result<T, TContext> : Result<T>
{
    /// <summary>
    /// Gets the resolved context data. Default value if validation failed.
    /// </summary>
    public TContext Context { get; }

    private Result(T value, TContext context)
        : base(value) => Context = context;

    private Result(IReadOnlyList<ValidationError> errors)
        : base(errors) => Context = default!;

    /// <summary>
    /// Creates a successful result with the given value and context.
    /// </summary>
    public static Result<T, TContext> Success(T value, TContext context) => new(value, context);

    /// <summary>
    /// Creates a failed result with the given errors.
    /// </summary>
    public new static Result<T, TContext> Failure(IReadOnlyList<ValidationError> errors) => new(errors);

    /// <summary>
    /// Creates a failed result with the given errors.
    /// </summary>
    public new static Result<T, TContext> Failure(params ValidationError[] errors) => new(errors);
}

/// <summary>
/// Exception thrown when GetOrThrow is called on a failed result.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors that caused the exception.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The validation errors.</param>
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
        List<ValidationError>? errors = null;
        List<T>? values = null;

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                errors ??= new List<ValidationError>();
                errors.AddRange(result.Errors);
            }
            else
            {
                values ??= new List<T>();
                values.Add(result.Value);
            }
        }

        return errors != null
            ? Result<T[]>.Failure(errors)
            : Result<T[]>.Success(values?.ToArray() ?? Array.Empty<T>());
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
