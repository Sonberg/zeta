```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   129.1 ns |  0.92 ns |  0.77 ns |  0.40 |    0.00 |    1 | 0.0715 |      - |     600 B |        2.42 |
| FluentValidation_Valid_Async   |   227.9 ns |  2.06 ns |  1.82 ns |  0.70 |    0.01 |    2 | 0.0801 |      - |     672 B |        2.71 |
| Zeta_Valid                     |   325.8 ns |  1.83 ns |  1.62 ns |  1.00 |    0.01 |    3 | 0.0296 |      - |     248 B |        1.00 |
| Zeta_Invalid                   |   450.4 ns |  4.37 ns |  4.09 ns |  1.38 |    0.01 |    4 | 0.1307 |      - |    1096 B |        4.42 |
| DataAnnotations_Valid          |   610.5 ns |  3.61 ns |  3.02 ns |  1.87 |    0.01 |    5 | 0.2203 |      - |    1848 B |        7.45 |
| DataAnnotations_Invalid        |   964.8 ns | 15.04 ns | 22.04 ns |  2.96 |    0.07 |    6 | 0.3185 | 0.0019 |    2672 B |       10.77 |
| FluentValidation_Invalid       | 1,903.0 ns | 16.17 ns | 13.51 ns |  5.84 |    0.05 |    7 | 0.9232 | 0.0095 |    7728 B |       31.16 |
| FluentValidation_Invalid_Async | 2,095.4 ns | 12.21 ns | 10.82 ns |  6.43 |    0.04 |    8 | 0.9308 | 0.0076 |    7800 B |       31.45 |
