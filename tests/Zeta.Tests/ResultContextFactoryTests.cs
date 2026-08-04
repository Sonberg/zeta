using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeta.Tests;

public class ResultContextFactoryTests
{
    private sealed record UpdateOrder(int OrderId);
    private sealed record OrderContext(bool IsOpen);

    private static ValidationRun RunWithServices()
        => new ValidationRun(serviceProvider: new ServiceCollection().BuildServiceProvider());

    [Fact]
    public async Task FailingResultFactory_SurfacesAsAggregatedError_NotException()
    {
        ISchema<UpdateOrder> schema = Z.Schema<UpdateOrder>()
            .Property(o => o.OrderId, s => s.Min(1))
            .Using<OrderContext>((req, _, _) =>
                ValueTask.FromResult(req.OrderId == 42
                    ? Result<OrderContext>.Success(new OrderContext(true))
                    : Result<OrderContext>.Failure(new ValidationError("$.orderId", "not_found", "Order not found"))))
            .Refine((_, ctx) => ctx.IsOpen, "Order is not open", "order_closed");

        var result = await schema.ValidateAsync(new UpdateOrder(99), RunWithServices());

        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal("not_found", error.Code);
        Assert.Equal("Order not found", error.Message);
        Assert.Equal("$.orderId", error.PathString);
    }

    [Fact]
    public async Task SucceedingResultFactory_ValidatesNormally()
    {
        ISchema<UpdateOrder> schema = Z.Schema<UpdateOrder>()
            .Using<OrderContext>((_, _, _) =>
                ValueTask.FromResult(Result<OrderContext>.Success(new OrderContext(true))))
            .Refine((_, ctx) => ctx.IsOpen, "Order is not open", "order_closed");

        var ok = await schema.ValidateAsync(new UpdateOrder(42), RunWithServices());
        Assert.True(ok.IsSuccess);
    }

    [Fact]
    public async Task SucceedingFactory_ButFailingRule_ReportsRuleError()
    {
        ISchema<UpdateOrder> schema = Z.Schema<UpdateOrder>()
            .Using<OrderContext>((_, _, _) =>
                ValueTask.FromResult(Result<OrderContext>.Success(new OrderContext(false))))
            .Refine((_, ctx) => ctx.IsOpen, "Order is not open", "order_closed");

        var result = await schema.ValidateAsync(new UpdateOrder(42), RunWithServices());

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "order_closed");
    }
}
