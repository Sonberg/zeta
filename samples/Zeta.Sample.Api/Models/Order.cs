namespace Zeta.Sample.Api.Models;

public record CreateOrderRequest(
    Guid CustomerId,
    OrderItemDto[] Items,
    AddressDto ShippingAddress,
    AddressDto? BillingAddress,
    string? CouponCode,
    string PaymentMethod);

public record OrderItemDto(
    Guid ProductId,
    int Quantity,
    string? Notes);

public record ScheduleDeliveryRequest(
    Guid OrderId,
    DateOnly DeliveryDate,
    TimeOnly? PreferredTimeStart,
    TimeOnly? PreferredTimeEnd);
