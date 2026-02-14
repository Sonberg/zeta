namespace Zeta.Sample.Api.Validation;

// Context for product creation - checks SKU uniqueness
public record CreateProductContext(bool SkuExists);
