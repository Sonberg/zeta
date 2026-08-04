# Benchmarks

Zeta ships a [BenchmarkDotNet](https://benchmarkdotnet.org/) suite that measures throughput and
allocations against [FluentValidation](https://docs.fluentvalidation.net/) and
[DataAnnotations](https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations),
on both simple schemas and nested object graphs.

## Running the benchmarks

```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release
```

This runs every `*Benchmarks` class in the assembly. Pass `--filter` to run a single class:

```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release -- --filter *ComplexValidationBenchmarks*
```

The source lives in [`benchmarks/Zeta.Benchmarks`](https://github.com/Sonberg/zeta/tree/main/benchmarks/Zeta.Benchmarks):

- **`ValidationBenchmarks`** — a flat DTO (name, email, age) validated with Zeta, FluentValidation
  and DataAnnotations, on both valid and invalid input.
- **`ComplexValidationBenchmarks`** — a realistic order: a nested address object, a collection of
  line items, a nullable conditional field and a cross-field `Refine` rule. DataAnnotations is
  omitted here — `Validator.TryValidateObject` doesn't recurse into nested objects or collection
  elements, so it can't express the scenario.
- **`AllocationBenchmarks`** — Zeta schemas in isolation (`[MemoryDiagnoser]`, `[ShortRunJob]`), to
  see the allocation cost of chaining rules on a single value schema without a comparison baseline.

## Results

Measured on .NET 10 (Apple M2 Pro). Your numbers will vary by hardware and .NET version — run the
suite yourself for numbers that match your environment.

### Flat DTO

| Method | Mean | Allocated | vs. Zeta |
|---|---:|---:|---:|
| FluentValidation (valid) | 137.6 ns | 600 B | 0.48x |
| FluentValidation (valid, async) | 236.3 ns | 672 B | 0.82x |
| **Zeta (valid)** | **287.6 ns** | **72 B** | 1.00x |
| Zeta (invalid) | 515.9 ns | 1,424 B | 1.79x |
| DataAnnotations (valid) | 605.3 ns | 1,848 B | 2.10x |
| DataAnnotations (invalid) | 1,015.9 ns | 2,672 B | 3.53x |
| FluentValidation (invalid) | 1,913.6 ns | 7,312 B | 6.65x |
| FluentValidation (invalid, async) | 2,068.4 ns | 7,384 B | 7.19x |

On valid input Zeta allocates 88% less than FluentValidation. On invalid input — the case where
error objects have to be built and aggregated — Zeta is 3.7x faster than FluentValidation's sync
API and allocates 5.1x less.

### Nested object graph

The order schema: address, line items, a nullable discount code, and a `Refine` rule checking the
total against the line sum.

| Method | Mean | Allocated | vs. Zeta |
|---|---:|---:|---:|
| **Zeta (valid)** | **1.606 μs** | **952 B** | 1.00x |
| FluentValidation (valid) | 1.778 μs | 4,904 B | 1.11x |
| Zeta (invalid) | 2.280 μs | 6,856 B | 1.42x |
| FluentValidation (invalid) | 5.806 μs | 18,136 B | 3.62x |

The gap widens on nested graphs: Zeta allocates 5.1x less than FluentValidation on valid input, and
is 2.5x faster with 2.6x less memory when validation fails.

### Rule chains in isolation

Allocation cost of chaining rules on a single `Z.String()`/`Z.Int()` schema, with no comparison
baseline:

| Method | Mean | Allocated |
|---|---:|---:|
| `Z.Int().Min(0).Max(120)` | 39.17 ns | 32 B |
| `Z.String().Email().MinLength(5).MaxLength(100)` | 98.72 ns | 0 B |
| `Z.String().MinLength(3).MaxLength(50).StartsWith(...).EndsWith(...).Contains(...).Email()` | 128.87 ns | 0 B |

String rules run allocation-free once a match succeeds; the `Int` schema's 32 B comes from boxing
the value passed into `Min`/`Max`.

## Methodology notes

- `ValidationBenchmarks` and `ComplexValidationBenchmarks` use `[MemoryDiagnoser]` with
  BenchmarkDotNet's default job (multiple iterations, no `[ShortRunJob]`), so the numbers above are
  full-precision results, not quick estimates.
- All Zeta benchmarks call `ValidateAsync(value, ValidationRun.Empty)` directly — no ASP.NET Core or
  FastEndpoints pipeline overhead is included.
- FluentValidation is benchmarked with both its sync `Validate` and async `ValidateAsync` entry
  points, since most real call sites use one or the other exclusively.
