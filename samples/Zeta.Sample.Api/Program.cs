using Zeta;
using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Repository;
using Zeta.Sample.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI for Swagger support
builder.Services.AddOpenApi();

// Register Zeta validation services
// - Registers IZetaValidator for manual validation in controllers
// - Scans assembly for IValidationContextFactory implementations and registers them
builder.Services.AddZeta(typeof(Program).Assembly);

// Register controllers
builder.Services.AddControllers();

// Register fake repositories (replace with real implementations)
builder.Services.AddScoped<IUserRepository, FakeUserRepository>();
builder.Services.AddScoped<IProductRepository, FakeProductRepository>();
builder.Services.AddScoped<IOrderRepository, FakeOrderRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// =====================
// MINIMAL API ENDPOINTS
// =====================

// Simple validation - no async context
app.MapPost("/api/minimal/users/register", (RegisterUserRequest request) =>
        Results.Ok(new
        {
            Message = "User registered",
            Email = request.Email
        }))
    .WithValidation(Schemas.RegisterUserSimple)
    .WithName("MinimalRegisterSimple")
    .WithTags("Minimal API - Users");

// Context-aware validation - async email uniqueness check
app.MapPost("/api/minimal/users/register/async", (RegisterUserRequest request) =>
        Results.Ok(new
        {
            Message = "User registered (with uniqueness check)",
            Email = request.Email
        }))
    .WithValidation(Schemas.RegisterUser)
    .WithName("MinimalRegisterAsync")
    .WithTags("Minimal API - Users");

// Conditional validation - address only validated when HasAddress is true
app.MapPost("/api/minimal/users", (CreateUserRequest request) =>
        Results.Created($"/api/minimal/users/{Guid.NewGuid()}", new
        {
            Message = "User created",
            User = request
        }))
    .WithValidation(Schemas.CreateUser)
    .WithName("MinimalCreateUser")
    .WithTags("Minimal API - Users");

// Simple product validation
app.MapPost("/api/minimal/products", (CreateProductRequest request) =>
        Results.Created($"/api/minimal/products/{Guid.NewGuid()}", new
        {
            Message = "Product created",
            Product = request
        }))
    .WithValidation(Schemas.CreateProductSimple)
    .WithName("MinimalCreateProduct")
    .WithTags("Minimal API - Products");

// Context-aware product validation - async SKU uniqueness
app.MapPost("/api/minimal/products/async", (CreateProductRequest request) =>
        Results.Created($"/api/minimal/products/{Guid.NewGuid()}", new
        {
            Message = "Product created (SKU checked)",
            Product = request
        }))
    .WithValidation(Schemas.CreateProduct)
    .WithName("MinimalCreateProductAsync")
    .WithTags("Minimal API - Products");

// Cross-field validation - compare price vs sale price
app.MapPatch("/api/minimal/products/{id:guid}/price", (Guid id, UpdatePriceRequest request) =>
        Results.Ok(new
        {
            Message = "Price updated",
            ProductId = id,
            request.Price,
            request.CompareAtPrice
        }))
    .WithValidation(Schemas.UpdatePrice)
    .WithName("MinimalUpdatePrice")
    .WithTags("Minimal API - Products");

// Query parameter validation with pagination
app.MapGet("/api/minimal/products/search", ([AsParameters] ProductSearchRequest request) =>
    Results.Ok(new
    {
        Message = "Search results",
        request.Query,
        request.Page,
        request.PageSize
    }))
    .WithValidation(Schemas.ProductSearch)
    .WithName("MinimalSearchProducts")
    .WithTags("Minimal API - Products");

// Complex nested validation - order with items and addresses
app.MapPost("/api/minimal/orders", (CreateOrderRequest request) =>
        Results.Created($"/api/minimal/orders/{Guid.NewGuid()}", new
        {
            Message = "Order created",
            request.CustomerId,
            ItemCount = request.Items.Length
        }))
    .WithValidation(Schemas.CreateOrder)
    .WithName("MinimalCreateOrder")
    .WithTags("Minimal API - Orders");

// Date/Time validation
app.MapPost("/api/minimal/orders/delivery", (ScheduleDeliveryRequest request) =>
        Results.Ok(new
        {
            Message = "Delivery scheduled",
            request.OrderId,
            request.DeliveryDate
        }))
    .WithValidation(Schemas.ScheduleDelivery)
    .WithName("MinimalScheduleDelivery")
    .WithTags("Minimal API - Orders");

// Map controllers
app.MapControllers();

app.Run();

// Context-aware schema: the object schema must be promoted when using context-aware fields
var contextAwareSchema = Z
    .Object<User>()
    .WithContext<User, UserContext>()
    .Field(f => f.Email, Z.String().WithContext<UserContext>().Email())
    .Field(f => f.Password, Z.String().MinLength(6).MaxLength(100));

// Contextless schema: validation rules can be applied before WithContext
var contextlessSchema = Z
    .Object<User>()
    .Field(f => f.Email, Z.String().Email())
    .Field(f => f.Password, Z.String().MinLength(6).MaxLength(100));

public record User(string Email, string Password);

public class UserContext(bool AllowExistingEmails);

// Required for WebApplicationFactory in integration tests
public partial class Program;