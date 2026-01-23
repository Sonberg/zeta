using Microsoft.AspNetCore.Mvc;
using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Validation;

using ValidationSchemas = Zeta.Sample.Api.Validation.Schemas;

namespace Zeta.Sample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IZetaValidator _validator;

    public OrdersController(IZetaValidator validator) => _validator = validator;

    /// <summary>
    /// Create order with full nested validation.
    /// Validates customer exists, products exist, coupon validity, and address format.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.CreateOrder, ct);

        return result.ToActionResult(valid => CreatedAtAction(
            nameof(Create),
            new
            {
                Message = "Order created",
                CustomerId = valid.CustomerId,
                ItemCount = valid.Items.Length,
                PaymentMethod = valid.PaymentMethod,
                HasCoupon = !string.IsNullOrEmpty(valid.CouponCode)
            }));
    }

    /// <summary>
    /// Schedule delivery with date/time validation.
    /// </summary>
    [HttpPost("delivery")]
    public async Task<IActionResult> ScheduleDelivery(ScheduleDeliveryRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ValidationSchemas.ScheduleDelivery, ct);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "Delivery scheduled",
            OrderId = valid.OrderId,
            DeliveryDate = valid.DeliveryDate,
            TimeWindow = valid.PreferredTimeStart.HasValue && valid.PreferredTimeEnd.HasValue
                ? $"{valid.PreferredTimeStart} - {valid.PreferredTimeEnd}"
                : "Any time"
        }));
    }
}
