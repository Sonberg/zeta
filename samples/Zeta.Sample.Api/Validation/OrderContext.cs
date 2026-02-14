namespace Zeta.Sample.Api.Validation;

// Context for order creation - validates customer exists and coupon validity
public record CreateOrderContext(
    bool CustomerExists,
    bool CouponValid,
    HashSet<Guid> ValidProductIds);
