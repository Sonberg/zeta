using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluentValidation;

namespace Zeta.Benchmarks;

// Realistic e-commerce order: nested object (address), collection of objects (lines),
// a nullable conditional field (discount) and a cross-field rule (total == sum of lines).
// DataAnnotations is omitted here — TryValidateObject does not recurse into nested
// objects or collection elements, so it can't express this scenario fairly.
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ComplexValidationBenchmarks
{
    private static readonly OrderDto _validOrder = new(
        Email: "buyer@example.com",
        Shipping: new AddressDto("1 Main St", "Springfield", "12345", "US"),
        Lines: new List<OrderLineDto>
        {
            new("SKU-001", 2, 9.99m),
            new("SKU-002", 1, 4.50m),
            new("SKU-003", 3, 1.00m),
        },
        DiscountCode: "SAVE10",
        Total: 2 * 9.99m + 4.50m + 3 * 1.00m);

    private static readonly OrderDto _invalidOrder = new(
        Email: "not-an-email",
        Shipping: new AddressDto("", "Springfield", "BAD", "USA"),   // empty street, bad zip, 3-char country
        Lines: new List<OrderLineDto>
        {
            new("", 0, -1m),   // empty sku, zero qty, negative price
        },
        DiscountCode: "x",     // too short
        Total: 999m);          // does not match line sum

    private readonly ISchema<OrderDto> _zetaSchema = BuildZetaSchema();

    private readonly OrderFluentValidator _fluentValidator = new();

    private static ISchema<OrderDto> BuildZetaSchema()
    {
        var address = Z.Schema<AddressDto>()
            .Property(a => a.Street, s => s.NotEmpty().MaxLength(100))
            .Property(a => a.City, s => s.NotEmpty().MaxLength(100))
            .Property(a => a.ZipCode, s => s.Regex(@"^\d{5}$"))
            .Property(a => a.Country, s => s.Length(2));

        var line = Z.Schema<OrderLineDto>()
            .Property(l => l.Sku, s => s.NotEmpty().MaxLength(20))
            .Property(l => l.Quantity, s => s.Min(1).Max(1000))
            .Property(l => l.UnitPrice, s => s.Min(0m));

        return Z.Schema<OrderDto>()
            .Property(o => o.Email, s => s.Email())
            .Property(o => o.Shipping, address)
            .Property(o => o.Lines, c => c.Each(line).MinLength(1).MaxLength(100))
            .Property(o => o.DiscountCode, s => s.MinLength(4).MaxLength(20).Nullable())
            .Refine(
                o => o.Total == o.Lines.Sum(l => l.Quantity * l.UnitPrice),
                "Total does not match the sum of line items",
                "total_mismatch");
    }

    [Benchmark(Baseline = true)]
    public async Task<bool> Zeta_Valid()
    {
        var result = await _zetaSchema.ValidateAsync(_validOrder, ValidationContext.Empty);
        return result.IsSuccess;
    }

    [Benchmark]
    public bool FluentValidation_Valid()
        => _fluentValidator.Validate(_validOrder).IsValid;

    [Benchmark]
    public async Task<int> Zeta_Invalid()
    {
        var result = await _zetaSchema.ValidateAsync(_invalidOrder, ValidationContext.Empty);
        return result.Errors.Count;
    }

    [Benchmark]
    public int FluentValidation_Invalid()
        => _fluentValidator.Validate(_invalidOrder).Errors.Count;
}

// ===== TEST MODELS =====

public record OrderDto(
    string Email,
    AddressDto Shipping,
    List<OrderLineDto> Lines,
    string? DiscountCode,
    decimal Total);

public record AddressDto(string Street, string City, string ZipCode, string Country);

public record OrderLineDto(string Sku, int Quantity, decimal UnitPrice);

// ===== FLUENT VALIDATION =====

public class OrderFluentValidator : AbstractValidator<OrderDto>
{
    public OrderFluentValidator()
    {
        RuleFor(o => o.Email).EmailAddress();
        RuleFor(o => o.Shipping).SetValidator(new AddressFluentValidator());
        RuleFor(o => o.Lines).NotEmpty().Must(l => l.Count <= 100);
        RuleForEach(o => o.Lines).SetValidator(new OrderLineFluentValidator());
        RuleFor(o => o.DiscountCode).MinimumLength(4).MaximumLength(20)
            .When(o => o.DiscountCode is not null);
        RuleFor(o => o).Must(o => o.Total == o.Lines.Sum(l => l.Quantity * l.UnitPrice))
            .WithMessage("Total does not match the sum of line items");
    }
}

public class AddressFluentValidator : AbstractValidator<AddressDto>
{
    public AddressFluentValidator()
    {
        RuleFor(a => a.Street).NotEmpty().MaximumLength(100);
        RuleFor(a => a.City).NotEmpty().MaximumLength(100);
        RuleFor(a => a.ZipCode).Matches(@"^\d{5}$");
        RuleFor(a => a.Country).Length(2);
    }
}

public class OrderLineFluentValidator : AbstractValidator<OrderLineDto>
{
    public OrderLineFluentValidator()
    {
        RuleFor(l => l.Sku).NotEmpty().MaximumLength(20);
        RuleFor(l => l.Quantity).InclusiveBetween(1, 1000);
        RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0m);
    }
}
