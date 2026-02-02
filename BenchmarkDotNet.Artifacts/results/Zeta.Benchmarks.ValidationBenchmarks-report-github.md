```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   131.5 ns |  1.83 ns |  2.10 ns |  0.39 |    0.01 |    1 | 0.0715 |      - |     600 B |        2.42 |
| FluentValidation_Valid_Async   |   228.5 ns |  0.78 ns |  0.61 ns |  0.68 |    0.00 |    2 | 0.0801 |      - |     672 B |        2.71 |
| Zeta_Valid                     |   333.9 ns |  1.41 ns |  1.18 ns |  1.00 |    0.00 |    3 | 0.0296 |      - |     248 B |        1.00 |
| Zeta_Invalid                   |   458.1 ns |  2.32 ns |  1.94 ns |  1.37 |    0.01 |    4 | 0.1307 |      - |    1096 B |        4.42 |
| DataAnnotations_Valid          |   613.0 ns |  2.22 ns |  1.85 ns |  1.84 |    0.01 |    5 | 0.2203 |      - |    1848 B |        7.45 |
| DataAnnotations_Invalid        |   967.1 ns |  5.37 ns |  5.02 ns |  2.90 |    0.02 |    6 | 0.3185 | 0.0019 |    2672 B |       10.77 |
| FluentValidation_Invalid       | 1,893.2 ns | 13.54 ns | 11.31 ns |  5.67 |    0.04 |    7 | 0.9232 | 0.0095 |    7728 B |       31.16 |
| FluentValidation_Invalid_Async | 2,073.9 ns | 14.73 ns | 12.30 ns |  6.21 |    0.04 |    8 | 0.9308 | 0.0076 |    7800 B |       31.45 |
