using Zeta.Sample.Api.Models;

namespace Zeta.Sample.Api.Validation;

/// <summary>
/// Central location for all validation schemas.
/// Schemas are defined as static fields for reuse across the application.
/// </summary>
public static class Schemas
{
    // =====================
    // Address Schema (Reusable)
    // =====================

    public static readonly ISchema<AddressDto> Address = Z.Object<AddressDto>()
        .Field(a => a.Street, Z.String().MinLength(5).MaxLength(200))
        .Field(a => a.City, Z.String().MinLength(2).MaxLength(100))
        .Field(a => a.State, Z.String().Length(2)) // US state code
        .Field(a => a.ZipCode, Z.String().Regex(@"^\d{5}(-\d{4})?$"))
        .Field(a => a.Country, Z.String().MaxLength(100).Nullable());

    // Context-aware address schema for orders
    public static ISchema<AddressDto, CreateOrderContext> AddressWithContext =>
        Z.Object<AddressDto, CreateOrderContext>()
            .Field(a => a.Street, Z.String<CreateOrderContext>().MinLength(5).MaxLength(200))
            .Field(a => a.City, Z.String<CreateOrderContext>().MinLength(2).MaxLength(100))
            .Field(a => a.State, Z.String<CreateOrderContext>().Length(2))
            .Field(a => a.ZipCode, Z.String<CreateOrderContext>().Regex(@"^\d{5}(-\d{4})?$"))
            .Field(a => a.Country, Z.String<CreateOrderContext>().MaxLength(100).Nullable());

    // =====================
    // User Schemas
    // =====================

    /// <summary>
    /// Registration with context-aware validation (async email uniqueness check).
    /// Uses cross-field validation for password confirmation.
    /// </summary>
    public static readonly ISchema<RegisterUserRequest, RegisterUserContext> RegisterUser =
        Z.Object<RegisterUserRequest, RegisterUserContext>()
            .Field(u => u.Email, Z.String<RegisterUserContext>()
                .Email()
                .Refine((email, ctx) => !ctx.EmailExists, "Email is already registered"))
            .Field(u => u.Password, Z.String<RegisterUserContext>()
                .MinLength(8)
                .Regex(@"[A-Z]", "Password must contain at least one uppercase letter")
                .Regex(@"[a-z]", "Password must contain at least one lowercase letter")
                .Regex(@"[0-9]", "Password must contain at least one digit"))
            .Field(u => u.ConfirmPassword, Z.String<RegisterUserContext>())
            .Field(u => u.Name, Z.String<RegisterUserContext>().MinLength(2).MaxLength(100).Nullable())
            .Field(u => u.Age, Z.Int<RegisterUserContext>().Min(13).Max(120))
            // Cross-field validation: passwords must match
            .Refine((u, _) => u.Password == u.ConfirmPassword, "Passwords do not match", "password_mismatch");

    /// <summary>
    /// Simple user registration (no async context).
    /// </summary>
    public static readonly ISchema<RegisterUserRequest> RegisterUserSimple =
        Z.Object<RegisterUserRequest>()
            .Field(u => u.Email, Z.String().Email())
            .Field(u => u.Password, Z.String().MinLength(8))
            .Field(u => u.ConfirmPassword, Z.String())
            .Field(u => u.Name, Z.String().MinLength(2).MaxLength(100).Nullable())
            .Field(u => u.Age, Z.Int().Min(13).Max(120))
            .Refine((u, _) => u.Password == u.ConfirmPassword, "Passwords do not match", "password_mismatch");

    /// <summary>
    /// User creation with conditional address validation.
    /// Address is only validated when HasAddress is true.
    /// </summary>
    public static readonly ISchema<CreateUserRequest> CreateUser =
        Z.Object<CreateUserRequest>()
            .Field(u => u.Email, Z.String().Email())
            .Field(u => u.Name, Z.String().MinLength(2).MaxLength(100))
            .When(
                u => u.HasAddress,
                then => then.Field(u => u.Address!, Address));

    /// <summary>
    /// Profile update - all fields optional but validated when present.
    /// </summary>
    public static readonly ISchema<UpdateProfileRequest> UpdateProfile =
        Z.Object<UpdateProfileRequest>()
            .Field(u => u.Name, Z.String().MinLength(2).MaxLength(100).Nullable())
            .Field(u => u.PhoneNumber, Z.String()
                .Regex(@"^\+?[1-9]\d{1,14}$", "Invalid phone number format")
                .Nullable())
            .Field(u => u.DateOfBirth, Z.DateOnly()
                .Refine((d, _) => d < DateOnly.FromDateTime(DateTime.Today), "Date of birth must be in the past")
                .Nullable());

    // =====================
    // Product Schemas
    // =====================

    /// <summary>
    /// Product creation with context-aware SKU uniqueness check.
    /// </summary>
    public static readonly ISchema<CreateProductRequest, CreateProductContext> CreateProduct =
        Z.Object<CreateProductRequest, CreateProductContext>()
            .Field(p => p.Name, Z.String<CreateProductContext>().MinLength(2).MaxLength(200))
            .Field(p => p.Description, Z.String<CreateProductContext>().MaxLength(2000).Nullable())
            .Field(p => p.Sku, Z.String<CreateProductContext>()
                .Regex(@"^[A-Z0-9\-]+$", "SKU must contain only uppercase letters, numbers, and hyphens")
                .Refine((sku, ctx) => !ctx.SkuExists, "SKU already exists"))
            .Field(p => p.Price, Z.Decimal<CreateProductContext>().Min(0.01m).Max(999999.99m))
            .Field(p => p.StockQuantity, Z.Int<CreateProductContext>().Min(0))
            .Field(p => p.Tags, Z.Array(Z.String<CreateProductContext>().MinLength(1).MaxLength(50)));

    /// <summary>
    /// Simple product creation (no async context).
    /// </summary>
    public static readonly ISchema<CreateProductRequest> CreateProductSimple =
        Z.Object<CreateProductRequest>()
            .Field(p => p.Name, Z.String().MinLength(2).MaxLength(200))
            .Field(p => p.Description, Z.String().MaxLength(2000).Nullable())
            .Field(p => p.Sku, Z.String().Regex(@"^[A-Z0-9\-]+$", "SKU must contain only uppercase letters, numbers, and hyphens"))
            .Field(p => p.Price, Z.Decimal().Min(0.01m).Max(999999.99m))
            .Field(p => p.StockQuantity, Z.Int().Min(0))
            .Field(p => p.Tags, Z.Array(Z.String().MinLength(1).MaxLength(50)));

    /// <summary>
    /// Price update with cross-field validation.
    /// CompareAtPrice must be greater than Price when present.
    /// </summary>
    public static readonly ISchema<UpdatePriceRequest> UpdatePrice =
        Z.Object<UpdatePriceRequest>()
            .Field(p => p.Price, Z.Decimal().Min(0.01m).Max(999999.99m))
            .Field(p => p.CompareAtPrice, Z.Decimal().Min(0.01m).Max(999999.99m).Nullable())
            .Refine(
                (p, _) => !p.CompareAtPrice.HasValue || p.CompareAtPrice > p.Price,
                "Compare-at price must be greater than the sale price",
                "invalid_compare_price");

    /// <summary>
    /// Product search with pagination validation.
    /// </summary>
    public static readonly ISchema<ProductSearchRequest> ProductSearch =
        Z.Object<ProductSearchRequest>()
            .Field(p => p.Query, Z.String().MaxLength(200).Nullable())
            .Field(p => p.MinPrice, Z.Decimal().Min(0).Nullable())
            .Field(p => p.MaxPrice, Z.Decimal().Min(0).Nullable())
            .Field(p => p.Page, Z.Int().Min(1))
            .Field(p => p.PageSize, Z.Int().Min(1).Max(100))
            .Refine(
                (p, _) => !p.MinPrice.HasValue || !p.MaxPrice.HasValue || p.MinPrice <= p.MaxPrice,
                "Maximum price must be greater than or equal to minimum price",
                "invalid_price_range");

    // =====================
    // Order Schemas
    // =====================

    /// <summary>
    /// Order item validation with context (validates product exists).
    /// </summary>
    private static ISchema<OrderItemDto, CreateOrderContext> OrderItem =>
        Z.Object<OrderItemDto, CreateOrderContext>()
            .Field(i => i.ProductId, Z.Guid<CreateOrderContext>()
                .Refine((id, ctx) => ctx.ValidProductIds.Contains(id), "Product not found"))
            .Field(i => i.Quantity, Z.Int<CreateOrderContext>().Min(1).Max(100))
            .Field(i => i.Notes, Z.String<CreateOrderContext>().MaxLength(500).Nullable());

    /// <summary>
    /// Full order creation with nested validation and context.
    /// </summary>
    public static readonly ISchema<CreateOrderRequest, CreateOrderContext> CreateOrder =
        Z.Object<CreateOrderRequest, CreateOrderContext>()
            .Field(o => o.CustomerId, Z.Guid<CreateOrderContext>()
                .Refine((_, ctx) => ctx.CustomerExists, "Customer not found"))
            .Field(o => o.Items, Z.Array(OrderItem)
                .Refine((items, _) => items.Length > 0, "Order must contain at least one item"))
            .Field(o => o.ShippingAddress, AddressWithContext)
            .Field(o => o.BillingAddress, Z.Object<AddressDto, CreateOrderContext>()
                .Field(a => a.Street, Z.String<CreateOrderContext>().MinLength(5).MaxLength(200))
                .Field(a => a.City, Z.String<CreateOrderContext>().MinLength(2).MaxLength(100))
                .Field(a => a.State, Z.String<CreateOrderContext>().Length(2))
                .Field(a => a.ZipCode, Z.String<CreateOrderContext>().Regex(@"^\d{5}(-\d{4})?$"))
                .Field(a => a.Country, Z.String<CreateOrderContext>().MaxLength(100).Nullable())
                .Nullable())
            .Field(o => o.CouponCode, Z.String<CreateOrderContext>()
                .Refine((code, ctx) => string.IsNullOrEmpty(code) || ctx.CouponValid, "Invalid coupon code")
                .Nullable())
            .Field(o => o.PaymentMethod, Z.String<CreateOrderContext>()
                .Refine((pm, _) => pm is "credit_card" or "paypal" or "bank_transfer",
                    "Payment method must be credit_card, paypal, or bank_transfer"));

    /// <summary>
    /// Delivery scheduling with date/time validation.
    /// </summary>
    public static readonly ISchema<ScheduleDeliveryRequest> ScheduleDelivery =
        Z.Object<ScheduleDeliveryRequest>()
            .Field(d => d.OrderId, Z.Guid())
            .Field(d => d.DeliveryDate, Z.DateOnly()
                .Refine((d, _) => d >= DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    "Delivery date must be at least tomorrow"))
            .Field(d => d.PreferredTimeStart, Z.TimeOnly().Nullable())
            .Field(d => d.PreferredTimeEnd, Z.TimeOnly().Nullable())
            .Refine(
                (d, _) => !d.PreferredTimeStart.HasValue || !d.PreferredTimeEnd.HasValue
                    || d.PreferredTimeEnd > d.PreferredTimeStart,
                "End time must be after start time",
                "invalid_time_range");
}
