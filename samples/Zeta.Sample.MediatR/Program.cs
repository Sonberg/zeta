using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zeta;
using Zeta.Core;

// Zeta + MediatR sample.
// Demonstrates: static schema-as-value contract, a Result-returning validation pipeline behavior,
// and a validation-aware context factory (#4) that turns "entity not found" into a normal
// aggregated validation error instead of a 500 or an ad-hoc "Exists" flag.

var services = new ServiceCollection();
services.AddScoped<IZetaValidator, ZetaValidator>();          // core Zeta — no ASP.NET reference needed
services.AddSingleton<IOrderRepo, InMemoryOrderRepo>();
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(ZetaValidationBehavior<,>));   // constrained open generic
});

using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

async Task Run<TResult>(string label, IRequest<Result<TResult>> request)
{
    var result = await mediator.Send(request);
    Console.WriteLine(result.IsSuccess
        ? $"[OK]   {label}: {result.Value}"
        : $"[FAIL] {label}: {string.Join("; ", result.Errors.Select(e => $"{e.PathString} {e.Code} \"{e.Message}\""))}");
}

await Run("valid user",           new CreateUserCommand("ada@example.com", 30));
await Run("invalid user",         new CreateUserCommand("not-an-email", 12));
await Run("update existing order", new UpdateOrderCommand(OrderId: 42, Note: "ship fast"));
await Run("update missing order",  new UpdateOrderCommand(OrderId: 99, Note: "ship fast"));

// ── Contract: a request that carries its own schema as a value ──────────────────
public interface IZetaValidation<T>
{
    static abstract ISchema<T> Schema { get; }
}

// ── Requests + colocated schemas ────────────────────────────────────────────────
public sealed record CreateUserCommand(string Email, int Age)
    : IRequest<Result<string>>, IZetaValidation<CreateUserCommand>
{
    public static ISchema<CreateUserCommand> Schema { get; } =
        Z.Schema<CreateUserCommand>()
            .Property(x => x.Email, Z.String().Email())
            .Property(x => x.Age, Z.Int().Min(18));
}

public sealed record UpdateOrderCommand(int OrderId, string Note)
    : IRequest<Result<string>>, IZetaValidation<UpdateOrderCommand>
{
    // Context-aware: the factory loads the order and fails validation cleanly if it is missing
    // (or closed) — the handler below never sees an invalid prerequisite.
    public static ISchema<UpdateOrderCommand> Schema { get; } =
        Z.Schema<UpdateOrderCommand>()
            .Property(x => x.Note, Z.String().NotEmpty().MaxLength(200))
            .Using<OrderContext>(async (cmd, sp, ct) =>
            {
                var order = await sp.GetRequiredService<IOrderRepo>().FindAsync(cmd.OrderId, ct);
                return order is null
                    ? Result<OrderContext>.Failure(new ValidationError("$.orderId", "not_found", $"Order {cmd.OrderId} not found"))
                    : Result<OrderContext>.Success(new OrderContext(order.IsOpen));
            })
            .Refine((_, ctx) => ctx.IsOpen, "Order is not open", "order_closed");
}

public sealed record OrderContext(bool IsOpen);

// ── Handlers (validation-free — input is already valid) ─────────────────────────
public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<string>>
{
    public Task<Result<string>> Handle(CreateUserCommand cmd, CancellationToken ct)
        => Task.FromResult(Result<string>.Success($"created user {cmd.Email}"));
}

public sealed class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<string>>
{
    public Task<Result<string>> Handle(UpdateOrderCommand cmd, CancellationToken ct)
        => Task.FromResult(Result<string>.Success($"updated order {cmd.OrderId}"));
}

// ── Validation behavior (Result-returning mode) ─────────────────────────────────
public sealed class ZetaValidationBehavior<TRequest, TResponse>(IZetaValidator validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IZetaValidation<TRequest>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, TRequest.Schema, ct);
        if (result.IsSuccess)
            return await next();

        // Build the handler's own Result<TResult>.Failure(...) — the documented `(TResponse)(object)result`
        // cast only works when the request and response are the same type, which they usually are not.
        return ResultFailure<TResponse>.From(result.Errors);
    }
}

// ponytail: reflection is fine for a sample behavior; a real app would constrain TResponse to a
// known result shape instead. Cached per closed TResponse so it runs once per response type.
internal static class ResultFailure<TResponse>
{
    private static readonly MethodInfo Failure = typeof(TResponse).GetMethod(
        "Failure", BindingFlags.Public | BindingFlags.Static, binder: null,
        types: [typeof(IReadOnlyList<ValidationError>)], modifiers: null)
        ?? throw new InvalidOperationException($"{typeof(TResponse)} is not a Zeta Result<T>.");

    public static TResponse From(IReadOnlyList<ValidationError> errors)
        => (TResponse)Failure.Invoke(null, [errors])!;
}

// ── Fake async repository (the "prerequisite" data source) ──────────────────────
public sealed record Order(int Id, bool IsOpen);

public interface IOrderRepo
{
    ValueTask<Order?> FindAsync(int id, CancellationToken ct);
}

public sealed class InMemoryOrderRepo : IOrderRepo
{
    private readonly Dictionary<int, Order> _orders = new() { [42] = new Order(42, IsOpen: true) };

    public ValueTask<Order?> FindAsync(int id, CancellationToken ct)
        => new(_orders.GetValueOrDefault(id));
}
