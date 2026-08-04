# Results and errors

Zeta never throws for invalid input. `ValidateAsync` returns a `Result<T>`, and invalid data is an
ordinary value you inspect — not an exception you catch.

```csharp
var result = await schema.ValidateAsync(input);
```

## Result&lt;T&gt;

| Member | Description |
|---|---|
| `IsSuccess` / `IsFailure` | Which case this is |
| `Value` | The validated value. Throws if the result is a failure |
| `Errors` | The collected `ValidationError`s. Empty on success |
| `Match(success, failure)` | Collapse both cases into one value |
| `Map(fn)` | Transform the value, leaving failures untouched |
| `Then(fn)` | Chain another operation that itself returns a result |
| `GetOrDefault(fallback)` | The value, or the fallback on failure |
| `GetOrThrow()` | The value, or throw `ValidationException` |

### Branching

```csharp
if (result.IsSuccess)
{
    await _repository.SaveAsync(result.Value);
}
else
{
    return BadRequest(result.Errors);
}
```

Or without the branch:

```csharp
var message = result.Match(
    success: user => $"Created {user.Email}",
    failure: errors => $"Rejected: {errors.Count} problems");
```

### Chaining

`Map` transforms a successful value; `Then` chains an operation that can itself fail. Both are
no-ops on a failed result, so a failure early in the chain flows straight through to the end:

```csharp
var response = await userSchema.ValidateAsync(input)
    .Then(user => SaveUserAsync(user))
    .Map(saved => new UserResponse(saved.Id));
```

### When to use GetOrThrow

`GetOrThrow()` exists for the boundary where you genuinely can't proceed — a startup configuration
check, a test assertion, a batch job that should abort. Reaching for it inside request handling
throws away the aggregated errors you just paid to compute.

## ValidationError

Every failure is a `ValidationError`:

| Member | Description |
|---|---|
| `Path` | Structured `ValidationPath` — segments, not text |
| `PathString` | Rendered path: `$.items[0].name` |
| `Code` | Machine-readable code, e.g. `min_length` |
| `Message` | Human-readable message |
| `AttemptedValue` | The value that failed, captured at the point of failure |
| `HasAttemptedValue` | Whether a value was captured |
| `TryGetAttemptedValue<T>(out T)` | Typed access to the attempted value |

```csharp
foreach (var error in result.Errors)
{
    Console.WriteLine($"{error.PathString}: {error.Code} — {error.Message}");
}
```

Use `PathString` for anything user-facing or logged, and `Path` when you're processing errors
programmatically — mapping to a form model, grouping by field, building a custom response shape.
See [Validation paths](/paths).

### Attempted values

`AttemptedValue` carries the actual rejected input, captured by the rule engine as the failure
happens:

```csharp
if (error.TryGetAttemptedValue<string>(out var attempted))
    _logger.LogWarning("Rejected {Value} at {Path}", attempted, error.PathString);
```

Because it's captured at the point of failure rather than re-resolved from the path afterwards, it's
correct even when paths are camel-cased or dictionary keys aren't strings — and it costs an allocation
only when validation actually fails.

## Errors are aggregated

Validation does not stop at the first failure. A schema with three broken properties returns three
errors in one pass, which is what a form or an API response needs:

```csharp
var result = await userSchema.ValidateAsync(new User("", "nope", 5));

result.Errors.Count;  // 3 — not 1
```

This holds through nesting too: a failing element deep inside a collection doesn't suppress errors
from its siblings.

## Grouping for API responses

`ToPathDictionary()` reshapes errors into the path-keyed dictionary that
`ValidationProblemDetails` expects:

```csharp
Dictionary<string, string[]> byPath = result.Errors.ToPathDictionary();
```

```json
{
  "$.email": ["Invalid email format"],
  "$.age": ["Must be at least 18"]
}
```

The [ASP.NET Core integration](/aspnetcore) does this for you.

## Combining results

`Combine` merges several results, accumulating every error rather than stopping at the first
failure:

```csharp
var combined = new[] { emailResult, nameResult, ageResult }.Combine();
```

## ValidationException

Thrown only by `GetOrThrow()`. It carries the `Errors` collection, so a handler at the top of your
stack can still produce a precise response:

```csharp
catch (ValidationException ex)
{
    return Results.ValidationProblem(ex.Errors.ToPathDictionary());
}
```

::: warning Context factory failures are not validation errors
If a [context factory](/validation-run) throws, that exception propagates — it surfaces as a 500, not
a 400. A factory that can legitimately fail should return a context that makes validation fail,
rather than throwing.
:::
