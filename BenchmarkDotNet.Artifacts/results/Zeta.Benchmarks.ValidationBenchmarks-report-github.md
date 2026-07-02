```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   137.6 ns |  2.64 ns |  2.21 ns |  0.48 |    0.01 |    1 | 0.0715 |      - |     600 B |        8.33 |
| FluentValidation_Valid_Async   |   236.3 ns |  4.03 ns |  3.77 ns |  0.82 |    0.01 |    2 | 0.0801 |      - |     672 B |        9.33 |
| Zeta_Valid                     |   287.6 ns |  0.99 ns |  0.88 ns |  1.00 |    0.00 |    3 | 0.0086 |      - |      72 B |        1.00 |
| Zeta_Invalid                   |   515.9 ns |  1.65 ns |  1.46 ns |  1.79 |    0.01 |    4 | 0.1698 |      - |    1424 B |       19.78 |
| DataAnnotations_Valid          |   605.3 ns |  2.99 ns |  2.65 ns |  2.10 |    0.01 |    5 | 0.2203 |      - |    1848 B |       25.67 |
| DataAnnotations_Invalid        | 1,015.9 ns |  4.52 ns |  4.00 ns |  3.53 |    0.02 |    6 | 0.3185 | 0.0019 |    2672 B |       37.11 |
| FluentValidation_Invalid       | 1,913.6 ns |  9.31 ns |  8.71 ns |  6.65 |    0.04 |    7 | 0.8736 | 0.0076 |    7312 B |      101.56 |
| FluentValidation_Invalid_Async | 2,068.4 ns | 19.56 ns | 17.34 ns |  7.19 |    0.06 |    8 | 0.8812 | 0.0076 |    7384 B |      102.56 |
