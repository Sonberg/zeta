namespace Zeta.Sample.Api.Repository;

public class FakeOrderRepository : IOrderRepository
{
    private static readonly HashSet<Guid> ExistingOrders = new()
    {
        Guid.Parse("11111111-0000-0000-0000-000000000001"),
        Guid.Parse("11111111-0000-0000-0000-000000000002")
    };

    private static readonly HashSet<string> ValidCoupons = new(StringComparer.OrdinalIgnoreCase)
    {
        "SAVE10",
        "WELCOME20",
        "FREESHIP"
    };

    public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken ct = default)
        => Task.FromResult(ExistingOrders.Contains(orderId));

    public Task<bool> CouponValidAsync(string couponCode, CancellationToken ct = default)
        => Task.FromResult(ValidCoupons.Contains(couponCode));
}
