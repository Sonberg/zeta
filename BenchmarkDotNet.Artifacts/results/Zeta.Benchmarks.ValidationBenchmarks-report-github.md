```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   129.1 ns |  1.84 ns |  1.73 ns |  0.44 |    0.01 |    1 | 0.0715 |      - |     600 B |        3.95 |
| FluentValidation_Valid_Async   |   229.3 ns |  1.16 ns |  0.97 ns |  0.78 |    0.00 |    2 | 0.0801 |      - |     672 B |        4.42 |
| Zeta_Valid                     |   293.2 ns |  1.71 ns |  1.42 ns |  1.00 |    0.01 |    3 | 0.0181 |      - |     152 B |        1.00 |
| Zeta_Invalid                   |   398.6 ns |  2.26 ns |  1.89 ns |  1.36 |    0.01 |    4 | 0.1078 |      - |     904 B |        5.95 |
| DataAnnotations_Valid          |   617.8 ns |  3.43 ns |  2.86 ns |  2.11 |    0.01 |    5 | 0.2203 |      - |    1848 B |       12.16 |
| DataAnnotations_Invalid        |   974.9 ns |  3.16 ns |  2.64 ns |  3.33 |    0.02 |    6 | 0.3185 | 0.0019 |    2672 B |       17.58 |
| FluentValidation_Invalid       | 1,920.5 ns | 11.99 ns | 10.01 ns |  6.55 |    0.04 |    7 | 0.9232 | 0.0076 |    7728 B |       50.84 |
| FluentValidation_Invalid_Async | 2,095.8 ns | 12.35 ns | 10.95 ns |  7.15 |    0.05 |    8 | 0.9308 | 0.0076 |    7800 B |       51.32 |
