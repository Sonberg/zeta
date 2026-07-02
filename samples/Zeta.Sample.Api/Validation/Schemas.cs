using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Repository;
using Zeta.Schemas;

namespace Zeta.Sample.Api.Validation;

/// <summary>
/// Central location for all validation schemas.
/// Schemas are defined as static fields for reuse across the application.
/// </summary>
public static class Schemas
{
    public static readonly ISchema<IFormFile> UploadFile =
        Z.Schema<IFormFile>()
            .Refine(f => !string.IsNullOrWhiteSpace(f.FileName), "File name is required", "file_name_required")
            .Refine(f => f.Length > 0, "File cannot be empty", "file_empty");

    // =====================
    // Address Schema (Reusable)
    // =====================

    public static readonly ISchema<AddressDto> Address = Z.Schema<AddressDto>()
        .Property(a => a.Street, Z.String().MinLength(5).MaxLength(200))
        .Property(a => a.City, Z.String().MinLength(2).MaxLength(100))
        .Property(a => a.State, Z.String().Length(2)) // US state code
        .Property(a => a.ZipCode, Z.String().Regex(@"^\d{5}(-\d{4})?$"))
        .Property(a => a.Country, Z.String().MaxLength(100).Nullable());

    // =====================
    // User Schemas
    // =====================

    /// <summary>
    /// Registration with context-aware validation (async email uniqueness check).
    /// Uses cross-field validation for password confirmation.
    /// </summary>
    public static readonly ISchema<RegisterUserRequest, RegisterUserContext> RegisterUser =
        Z.Schema<RegisterUserRequest>()
            .Using<RegisterUserContext>(async (value, sp, ct) =>
            {
                var repo = sp.GetRequiredService<IUserRepository>();
                var emailExists = await repo.EmailExistsAsync(value.Email, ct);
                return new RegisterUserContext(emailExists);
            })
            .Property(u => u.Email, Z.String().Email())
            .Property(u => u.Password, Z.String()
                .MinLength(8)
                .Regex(@"[A-Z]", "Password must contain at least one uppercase letter")
                .Regex(@"[a-z]", "Password must contain at least one lowercase letter")
                .Regex(@"[0-9]", "Password must contain at least one digit"))
            .Property(u => u.ConfirmPassword, Z.String())
            .Property(u => u.Name, Z.String().MinLength(2).MaxLength(100).Nullable())
            .Property(u => u.Age, Z.Int().Min(13).Max(120))
            // Context-aware: check email uniqueness
            .Refine((u, ctx) => !ctx.EmailExists, "Email is already registered")
            // Cross-field validation: passwords must match
            .Refine(u => u.Password == u.ConfirmPassword, "Passwords do not match", "password_mismatch");

    /// <summary>
    /// Simple user registration (no async context).
    /// </summary>
    public static readonly ISchema<RegisterUserRequest> RegisterUserSimple =
        Z.Schema<RegisterUserRequest>()
            .Property(u => u.Email, Z.String().Email())
            .Property(u => u.Password, Z.String().MinLength(8))
            .Property(u => u.ConfirmPassword, Z.String())
            .Property(u => u.Name, Z.String().MinLength(2).MaxLength(100).Nullable())
            .Property(u => u.Age, Z.Int().Min(13).Max(120))
            .Refine(u => u.Password == u.ConfirmPassword, "Passwords do not match", "password_mismatch");

    /// <summary>
    /// User creation with conditional address validation.
    /// Address is only validated when HasAddress is true.
    /// </summary>
    public static readonly ISchema<CreateUserRequest> CreateUser =
        Z.Schema<CreateUserRequest>()
            .Property(u => u.Email, Z.String().Email())
            .Property(u => u.Name, Z.String().MinLength(2).MaxLength(100));

    /// <summary>
    /// Profile update - all fields optional but validated when present.
    /// </summary>
    public static readonly ISchema<UpdateProfileRequest> UpdateProfile =
        Z.Schema<UpdateProfileRequest>()
            .Property(u => u.Name, Z.String().MinLength(2).MaxLength(100).Nullable())
            .Property(u => u.PhoneNumber, Z.String()
                .Regex(@"^\+?[1-9]\d{1,14}$", "Invalid phone number format")
                .Nullable())
            .Property(u => u.DateOfBirth, Z.DateOnly()
                .Refine(d => d < DateOnly.FromDateTime(DateTime.Today), "Date of birth must be in the past")
                .Nullable());

    // =====================
    // Product Schemas
    // =====================

    /// <summary>
    /// Product creation with context-aware SKU uniqueness check.
    /// </summary>
    public static readonly ISchema<CreateProductRequest> CreateProduct =
        Z.Schema<CreateProductRequest>()
            .Using<CreateProductContext>(async (value, sp, ct) =>
            {
                var repo = sp.GetRequiredService<IProductRepository>();
                var skuExists = await repo.SkuExistsAsync(value.Sku, ct);
                return new CreateProductContext(skuExists);
            })
            .Property(p => p.Name, Z.String().MinLength(2).MaxLength(200))
            .Property(p => p.Description, Z.String().MaxLength(2000).Nullable())
            .Property(p => p.Sku, Z.String()
                .Regex(@"^[A-Z0-9\-]+$", "SKU must contain only uppercase letters, numbers, and hyphens"))
            .Property(p => p.Price, Z.Decimal().Min(0.01m).Max(999999.99m))
            .Property(p => p.StockQuantity, Z.Int().Min(0))
            .Property(p => p.Tags, tags => tags.Each(s => s.MinLength(1).MaxLength(50)).Using<CreateProductContext>())
            .Refine((p, ctx) => !ctx.SkuExists, "SKU already exists");

    /// <summary>
    /// Simple product creation (no async context).
    /// </summary>
    public static readonly ISchema<CreateProductRequest> CreateProductSimple =
        Z.Schema<CreateProductRequest>()
            .Property(p => p.Name, Z.String().MinLength(2).MaxLength(200))
            .Property(p => p.Description, Z.String().MaxLength(2000).Nullable())
            .Property(p => p.Sku, Z.String().Regex(@"^[A-Z0-9\-]+$", "SKU must contain only uppercase letters, numbers, and hyphens"))
            .Property(p => p.Price, Z.Decimal().Min(0.01m).Max(999999.99m))
            .Property(p => p.StockQuantity, Z.Int().Min(0))
            .Property(p => p.Tags, tags => tags.Each(s => s.MinLength(1).MaxLength(50)));

    /// <summary>
    /// Price update with cross-field validation.
    /// CompareAtPrice must be greater than Price when present.
    /// </summary>
    public static readonly ISchema<UpdatePriceRequest> UpdatePrice =
        Z.Schema<UpdatePriceRequest>()
            .Property(p => p.Price, Z.Decimal().Min(0.01m).Max(999999.99m))
            .Property(p => p.CompareAtPrice, Z.Decimal().Min(0.01m).Max(999999.99m).Nullable())
            .Refine(
                p => !p.CompareAtPrice.HasValue || p.CompareAtPrice > p.Price,
                "Compare-at price must be greater than the sale price",
                "invalid_compare_price");

    /// <summary>
    /// Product search with pagination validation.
    /// </summary>
    public static readonly ISchema<ProductSearchRequest> ProductSearch =
        Z.Schema<ProductSearchRequest>()
            .Property(p => p.Query, Z.String().MaxLength(200).Nullable())
            .Property(p => p.MinPrice, Z.Decimal().Min(0).Nullable())
            .Property(p => p.MaxPrice, Z.Decimal().Min(0).Nullable())
            .Property(p => p.Page, Z.Int().Min(1))
            .Property(p => p.PageSize, Z.Int().Min(1).Max(100))
            .Refine(
                p => !p.MinPrice.HasValue || !p.MaxPrice.HasValue || p.MinPrice <= p.MaxPrice,
                "Maximum price must be greater than or equal to minimum price",
                "invalid_price_range");

    // =====================
    // Order Schemas
    // =====================

    /// <summary>
    /// Order item validation (basic, no context).
    /// </summary>
    private static readonly ISchema<OrderItemDto> OrderItemBasic =
        Z.Schema<OrderItemDto>()
            .Property(i => i.ProductId, Z.Guid())
            .Property(i => i.Quantity, Z.Int().Min(1).Max(100))
            .Property(i => i.Notes, Z.String().MaxLength(500).Nullable());

    /// <summary>
    /// Nullable address schema for optional billing address.
    /// </summary>
    private static readonly ObjectContextlessSchema<AddressDto> AddressNullable =
        Z.Schema<AddressDto>()
            .Property(a => a.Street, Z.String().MinLength(5).MaxLength(200))
            .Property(a => a.City, Z.String().MinLength(2).MaxLength(100))
            .Property(a => a.State, Z.String().Length(2))
            .Property(a => a.ZipCode, Z.String().Regex(@"^\d{5}(-\d{4})?$"))
            .Property(a => a.Country, Z.String().MaxLength(100).Nullable())
            .Nullable();

    /// <summary>
    /// Full order creation with nested validation and context.
    /// </summary>
    public static readonly ISchema<CreateOrderRequest, CreateOrderContext> CreateOrder =
        Z.Schema<CreateOrderRequest>()
            .Using<CreateOrderContext>(async (value, sp, ct) =>
            {
                var userRepo = sp.GetRequiredService<IUserRepository>();
                var productRepo = sp.GetRequiredService<IProductRepository>();
                var orderRepo = sp.GetRequiredService<IOrderRepository>();

                var customerExists = await userRepo.UserExistsAsync(value.CustomerId, ct);

                var couponValid = string.IsNullOrEmpty(value.CouponCode)
                                  || await orderRepo.CouponValidAsync(value.CouponCode, ct);

                var validProductIds = new HashSet<Guid>();
                foreach (var item in value.Items)
                {
                    if (await productRepo.ProductExistsAsync(item.ProductId, ct))
                        validProductIds.Add(item.ProductId);
                }

                return new CreateOrderContext(customerExists, couponValid, validProductIds);
            })
            .Property(o => o.CustomerId, Z.Guid())
            .Property(o => o.Items, Z.Collection(OrderItemBasic))
            .Property(o => o.ShippingAddress, Address)
            .Property(o => o.BillingAddress, AddressNullable)
            .Property(o => o.CouponCode, Z.String().Nullable())
            .Property(o => o.PaymentMethod, Z.String()
                .Refine(pm => pm is "credit_card" or "paypal" or "bank_transfer",
                    "Payment method must be credit_card, paypal, or bank_transfer"))
            // Context-aware validations
            .Refine((o, ctx) => ctx.CustomerExists, "Customer not found")
            .Refine((o, ctx) => o.Items.All(i => ctx.ValidProductIds.Contains(i.ProductId)), "One or more products not found")
            .Refine((o, ctx) => string.IsNullOrEmpty(o.CouponCode) || ctx.CouponValid, "Invalid coupon code")
            .Refine(o => o.Items.Length > 0, "Order must contain at least one item");

    /// <summary>
    /// Delivery scheduling with date/time validation.
    /// </summary>
    public static readonly ISchema<ScheduleDeliveryRequest> ScheduleDelivery =
        Z.Schema<ScheduleDeliveryRequest>()
            .Property(d => d.OrderId, Z.Guid())
            .Property(d => d.DeliveryDate, Z.DateOnly()
                .Refine(d => d >= DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    "Delivery date must be at least tomorrow"))
            .Property(d => d.PreferredTimeStart, Z.TimeOnly().Nullable())
            .Property(d => d.PreferredTimeEnd, Z.TimeOnly().Nullable())
            .Refine(
                d => !d.PreferredTimeStart.HasValue || !d.PreferredTimeEnd.HasValue
                    || d.PreferredTimeEnd > d.PreferredTimeStart,
                "End time must be after start time",
                "invalid_time_range");
}
