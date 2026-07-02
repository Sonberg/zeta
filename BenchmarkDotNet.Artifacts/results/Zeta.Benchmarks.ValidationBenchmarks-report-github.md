```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   134.0 ns |  2.46 ns |  2.18 ns |  0.47 |    0.01 |    1 | 0.0715 |      - |     600 B |        8.33 |
| FluentValidation_Valid_Async   |   228.2 ns |  1.01 ns |  0.90 ns |  0.81 |    0.00 |    2 | 0.0801 |      - |     672 B |        9.33 |
| Zeta_Valid                     |   283.1 ns |  1.68 ns |  1.40 ns |  1.00 |    0.01 |    3 | 0.0086 |      - |      72 B |        1.00 |
| Zeta_Invalid                   |   517.8 ns |  4.07 ns |  3.60 ns |  1.83 |    0.02 |    4 | 0.1698 |      - |    1424 B |       19.78 |
| DataAnnotations_Valid          |   606.4 ns |  2.89 ns |  2.70 ns |  2.14 |    0.01 |    5 | 0.2203 |      - |    1848 B |       25.67 |
| DataAnnotations_Invalid        | 1,024.6 ns |  6.42 ns |  5.02 ns |  3.62 |    0.02 |    6 | 0.3185 | 0.0019 |    2672 B |       37.11 |
| FluentValidation_Invalid       | 1,855.9 ns |  8.41 ns |  7.02 ns |  6.55 |    0.04 |    7 | 0.8736 | 0.0076 |    7312 B |      101.56 |
| FluentValidation_Invalid_Async | 2,028.4 ns | 40.13 ns | 41.21 ns |  7.16 |    0.15 |    8 | 0.8812 | 0.0076 |    7384 B |      102.56 |
