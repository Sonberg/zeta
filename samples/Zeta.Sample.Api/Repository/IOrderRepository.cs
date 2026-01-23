namespace Zeta.Sample.Api.Repository;

public interface IOrderRepository
{
    Task<bool> OrderExistsAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> CouponValidAsync(string couponCode, CancellationToken ct = default);
}
