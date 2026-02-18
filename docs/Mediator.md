# Zeta + Mediator Integration

This guide shows how to integrate **Zeta** with the **Mediator pattern** (e.g. [MediatR](https://github.com/jbogard/MediatR)) in a clean, idiomatic, and async-first way.

The recommended approach is to run validation in a **Mediator pipeline behavior**, keeping handlers pure and free of validation concerns.

---

## Why Zeta fits Mediator well

* **Async-first** – validation and context loading are async by design
* **Schema as values** – schemas live next to requests
* **Result-based** – no exceptions, no control-flow abuse
* **Context-aware** – async data loading fits pipeline behaviors naturally

---

## 1. Define a Validatable Request Contract

Create a marker interface that exposes a schema for the request:

```csharp
public interface IZetaValidation<T>
{
    static abstract ISchema<T> Schema { get; }
}
```

---

## 2. Define a Request with an Inline Schema

```csharp
public sealed record CreateUserCommand(
    string Email,
    int Age
) : IRequest<Result<User>>, IZetaValidation<CreateUserCommand>
{
    public static ISchema<CreateUserCommand> Schema { get; } =
        Z.Schema<CreateUserCommand>()
            .Property(x => x.Email, Z.String().Email())
            .Property(x => x.Age, Z.Int().Min(18));
}
```

Schemas are:

* strongly typed
* reusable
* testable
* colocated with the request

---

## 3. Validation Pipeline Behavior

Validation runs *before* the handler executes.

```csharp
public sealed class ZetaValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IZetaValidation<TRequest>
{
    private readonly IZetaValidator _validator;

    public ZetaValidationBehavior(IZetaValidator validator)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(
            request,
            TRequest.Schema,
            ct
        );

        if (result.IsFailure)
        {
            // Handlers returning Result<T>
            return (TResponse)(object)result;
        }

        return await next();
    }
}
```

---

## 4. Register the Pipeline

```csharp
services.AddMediatR(cfg =>
{
    cfg.AddOpenBehavior(typeof(ZetaValidationBehavior<,>));
});
```

---

## 5. Handler (Validation-Free)

```csharp
public sealed class CreateUserHandler
    : IRequestHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> Handle(
        CreateUserCommand cmd,
        CancellationToken ct)
    {
        // Input is guaranteed to be valid
        var user = new User(cmd.Email, cmd.Age);
        return Result.Success(user);
    }
}
```

No validation logic. No duplication. No exceptions.

---

## 6. Context-Aware Validation (Async Data)

Zeta supports async context loading *before* validation.

### Request with Context

```csharp
public sealed record RegisterUserCommand(
    string Email,
    string Password
) : IRequest<Result<User>>, IZetaValidation<RegisterUserCommand>
{
    public static ISchema<RegisterUserCommand> Schema { get; } =
        Z.Schema<RegisterUserCommand>()
            .Using<UserContext>(async (input, sp, ct) =>
            {
                var repo = sp.GetRequiredService<IUserRepository>();
                return new UserContext(
                    EmailExists: await repo.EmailExistsAsync(input.Email, ct)
                );
            })
            .Property(x => x.Email,
                Z.String()
                    .Email()
                    .Using<UserContext>()
                    .Refine((_, ctx) => !ctx.EmailExists, "Email already taken")
            )
            .Property(x => x.Password, Z.String().MinLength(8));
}
```

---

## 7. Optional: Manual Validation Inside Handlers

For orchestration-heavy workflows:

```csharp
public sealed class CreateBookingHandler
    : IRequestHandler<CreateBooking, Result<Booking>>
{
    private static readonly ISchema<CreateBooking> Schema =
        Z.Schema<CreateBooking>()
            .Property(x => x.Date, Z.DateTime().Future());

    private readonly IZetaValidator _validator;

    public CreateBookingHandler(IZetaValidator validator)
    {
        _validator = validator;
    }

    public async Task<Result<Booking>> Handle(
        CreateBooking cmd,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(cmd, Schema, ct);

        return await validation.ThenAsync(valid =>
        {
            return Task.FromResult(new Booking(valid.Date));
        });
    }
}
```

---

## Summary

**Recommended setup**:

* Schemas live on requests
* Validation runs in a Mediator pipeline
* Handlers assume valid input
* Async context loading via factories
* No exceptions, no duplication

This approach scales cleanly from simple commands to complex, context-aware validation pipelines.

---

Happy validating ✨
