using System.ComponentModel.DataAnnotations;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluentValidation;

namespace Zeta.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ValidationBenchmarks
{
    // Test data
    private readonly UserDto _validUser = new("John Doe", "john@example.com", 25);
    private readonly UserDto _invalidUser = new("Jo", "invalid-email", 15);

    // DataAnnotations models
    private readonly UserDtoAnnotated _validUserAnnotated = new() { Name = "John Doe", Email = "john@example.com", Age = 25 };
    private readonly UserDtoAnnotated _invalidUserAnnotated = new() { Name = "Jo", Email = "invalid-email", Age = 15 };

    // Zeta schemas
    private readonly ISchema<UserDto> _zetaSchema = Z.Object<UserDto>()
        .Field(u => u.Name, Z.String().MinLength(3).MaxLength(100))
        .Field(u => u.Email, Z.String().Email())
        .Field(u => u.Age, Z.Int().Min(18).Max(120));

    // FluentValidation validator
    private readonly FluentUserValidator _fluentValidator = new();

    // ===== VALID INPUT BENCHMARKS =====

    [Benchmark(Baseline = true)]
    public async Task<bool> Zeta_Valid()
    {
        var result = await _zetaSchema.ValidateAsync(_validUser, ValidationContext.Empty);
        return result.IsSuccess;
    }

    [Benchmark]
    public bool FluentValidation_Valid()
    {
        var result = _fluentValidator.Validate(_validUser);
        return result.IsValid;
    }

    [Benchmark]
    public async Task<bool> FluentValidation_Valid_Async()
    {
        var result = await _fluentValidator.ValidateAsync(_validUser);
        return result.IsValid;
    }

    [Benchmark]
    public bool DataAnnotations_Valid()
    {
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(_validUserAnnotated);
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(_validUserAnnotated, context, results, validateAllProperties: true);
    }

    // ===== INVALID INPUT BENCHMARKS =====

    [Benchmark]
    public async Task<int> Zeta_Invalid()
    {
        var result = await _zetaSchema.ValidateAsync(_invalidUser, ValidationContext.Empty);
        return result.Errors.Count;
    }

    [Benchmark]
    public int FluentValidation_Invalid()
    {
        var result = _fluentValidator.Validate(_invalidUser);
        return result.Errors.Count;
    }

    [Benchmark]
    public async Task<int> FluentValidation_Invalid_Async()
    {
        var result = await _fluentValidator.ValidateAsync(_invalidUser);
        return result.Errors.Count;
    }

    [Benchmark]
    public int DataAnnotations_Invalid()
    {
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(_invalidUserAnnotated);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(_invalidUserAnnotated, context, results, validateAllProperties: true);
        return results.Count;
    }
}

// ===== TEST MODELS =====

public record UserDto(string Name, string Email, int Age);

// ===== FLUENT VALIDATION =====

public class FluentUserValidator : AbstractValidator<UserDto>
{
    public FluentUserValidator()
    {
        RuleFor(u => u.Name).MinimumLength(3).MaximumLength(100);
        RuleFor(u => u.Email).EmailAddress();
        RuleFor(u => u.Age).InclusiveBetween(18, 120);
    }
}

// ===== DATA ANNOTATIONS MODEL =====

public class UserDtoAnnotated
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Range(18, 120)]
    public int Age { get; set; }
}
