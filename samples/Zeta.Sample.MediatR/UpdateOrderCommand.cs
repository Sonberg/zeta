using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.Sample.MediatR;

public sealed record OrderContext(bool IsOpen);

public sealed record UpdateOrderCommand(int OrderId, string Note) : IRequest<Result<string>>;

public sealed class UpdateOrderHandler(IZetaValidator validator) : IRequestHandler<UpdateOrderCommand, Result<string>>
{
    private static readonly ISchema<UpdateOrderCommand> Schema = Z
        .Schema<UpdateOrderCommand>()
        .Property(x => x.Note, Z.String().NotEmpty().MaxLength(200))
        .Using<OrderContext>(async (cmd, sp, ct) =>
        {
            var order = await sp.GetRequiredService<IOrderRepo>().FindAsync(cmd.OrderId, ct);

            return order is null
                ? Result<OrderContext>.Failure(new ValidationError("$.orderId", "not_found", $"Order {cmd.OrderId} not found"))
                : Result<OrderContext>.Success(new OrderContext(order.IsOpen));
        })
        .Refine((_, ctx) => ctx.IsOpen, "Order is not open", "order_closed");

    public async Task<Result<string>> Handle(UpdateOrderCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, Schema, ct);

        return validation.IsSuccess
            ? Result<string>.Success($"updated order {cmd.OrderId}")
            : Result<string>.Failure(validation.Errors);
    }
}