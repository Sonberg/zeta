using BenchmarkDotNet.Attributes;
using Zeta;
using Zeta.Core;

namespace Zeta.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class AllocationBenchmarks
{
    private readonly ISchema<string> _emailSchema = Z.String().Email().MinLength(5).MaxLength(100);
    private readonly string _validEmail = "test@example.com";

    [Benchmark]
    public async Task<bool> ValidateStringWithMultipleRules()
    {
        var result = await _emailSchema.ValidateAsync(_validEmail, ValidationContext.Empty);
        return result.IsSuccess;
    }

    private readonly ISchema<int> _ageSchema = Z.Int().Min(0).Max(120);
    private readonly int _validAge = 25;

    [Benchmark]
    public async Task<bool> ValidateIntWithMinMax()
    {
        var result = await _ageSchema.ValidateAsync(_validAge, ValidationContext.Empty);
        return result.IsSuccess;
    }

    private readonly ISchema<string> _complexStringSchema = Z.String()
        .MinLength(3)
        .MaxLength(50)
        .StartsWith("test")
        .EndsWith(".com")
        .Contains("@")
        .Email();

    [Benchmark]
    public async Task<bool> ValidateStringWithManyRules()
    {
        var result = await _complexStringSchema.ValidateAsync(_validEmail, ValidationContext.Empty);
        return result.IsSuccess;
    }
}
