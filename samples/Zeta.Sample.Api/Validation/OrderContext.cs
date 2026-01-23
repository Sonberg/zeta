using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Repository;

namespace Zeta.Sample.Api.Validation;

// Context for order creation - validates customer exists and coupon validity
public record CreateOrderContext(
    bool CustomerExists,
    bool CouponValid,
    HashSet<Guid> ValidProductIds);

public class CreateOrderContextFactory : IValidationContextFactory<CreateOrderRequest, CreateOrderContext>
{
    private readonly IUserRepository _userRepo;
    private readonly IProductRepository _productRepo;
    private readonly IOrderRepository _orderRepo;

    public CreateOrderContextFactory(
        IUserRepository userRepo,
        IProductRepository productRepo,
        IOrderRepository orderRepo)
    {
        _userRepo = userRepo;
        _productRepo = productRepo;
        _orderRepo = orderRepo;
    }

    public async Task<CreateOrderContext> CreateAsync(
        CreateOrderRequest input,
        IServiceProvider services,
        CancellationToken ct)
    {
        var customerExists = await _userRepo.UserExistsAsync(input.CustomerId, ct);

        var couponValid = string.IsNullOrEmpty(input.CouponCode)
            || await _orderRepo.CouponValidAsync(input.CouponCode, ct);

        // Check which product IDs are valid
        var validProductIds = new HashSet<Guid>();
        foreach (var item in input.Items)
        {
            if (await _productRepo.ProductExistsAsync(item.ProductId, ct))
                validProductIds.Add(item.ProductId);
        }

        return new CreateOrderContext(customerExists, couponValid, validProductIds);
    }
}
