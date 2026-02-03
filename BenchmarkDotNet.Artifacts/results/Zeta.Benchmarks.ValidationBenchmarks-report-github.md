```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev  | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|--------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   127.7 ns |  0.69 ns | 0.62 ns |  0.44 |    0.00 |    1 | 0.0715 |      - |     600 B |        3.95 |
| FluentValidation_Valid_Async   |   228.2 ns |  1.36 ns | 1.28 ns |  0.78 |    0.01 |    2 | 0.0801 |      - |     672 B |        4.42 |
| Zeta_Valid                     |   291.0 ns |  1.43 ns | 1.12 ns |  1.00 |    0.01 |    3 | 0.0181 |      - |     152 B |        1.00 |
| Zeta_Invalid                   |   459.6 ns |  2.98 ns | 2.64 ns |  1.58 |    0.01 |    4 | 0.1307 |      - |    1096 B |        7.21 |
| DataAnnotations_Valid          |   635.5 ns |  3.12 ns | 2.61 ns |  2.18 |    0.01 |    5 | 0.2203 |      - |    1848 B |       12.16 |
| DataAnnotations_Invalid        |   970.5 ns |  5.70 ns | 5.05 ns |  3.34 |    0.02 |    6 | 0.3185 | 0.0019 |    2672 B |       17.58 |
| FluentValidation_Invalid       | 1,907.6 ns | 10.15 ns | 7.93 ns |  6.56 |    0.04 |    7 | 0.9232 | 0.0076 |    7728 B |       50.84 |
| FluentValidation_Invalid_Async | 2,057.6 ns |  6.90 ns | 6.12 ns |  7.07 |    0.03 |    8 | 0.9308 | 0.0076 |    7800 B |       51.32 |
