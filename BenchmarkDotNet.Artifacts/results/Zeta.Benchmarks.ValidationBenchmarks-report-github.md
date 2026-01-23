```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.0, 9.0.24.52809), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 9.0.0 (9.0.0, 9.0.24.52809), Arm64 RyuJIT armv8.0-a


```
| Method                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------- |-----------:|---------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| FluentValidation_Valid         |   152.7 ns |  1.64 ns |  1.45 ns |  0.50 |    0.01 |    1 | 0.0715 |      - |     600 B |        0.88 |
| FluentValidation_Valid_Async   |   275.7 ns |  5.30 ns |  4.96 ns |  0.91 |    0.02 |    2 | 0.0801 |      - |     672 B |        0.99 |
| Zeta_Valid                     |   302.9 ns |  5.58 ns |  4.95 ns |  1.00 |    0.02 |    3 | 0.0811 |      - |     680 B |        1.00 |
| Zeta_Invalid                   |   444.4 ns |  3.42 ns |  2.86 ns |  1.47 |    0.02 |    4 | 0.1564 |      - |    1312 B |        1.93 |
| DataAnnotations_Valid          |   689.3 ns |  3.15 ns |  2.46 ns |  2.28 |    0.04 |    5 | 0.2241 |      - |    1880 B |        2.76 |
| DataAnnotations_Invalid        | 1,099.4 ns |  5.42 ns |  4.23 ns |  3.63 |    0.06 |    6 | 0.3223 |      - |    2704 B |        3.98 |
| FluentValidation_Invalid       | 2,266.1 ns |  9.13 ns |  7.62 ns |  7.48 |    0.12 |    7 | 0.9460 | 0.0076 |    7920 B |       11.65 |
| FluentValidation_Invalid_Async | 2,453.3 ns | 22.28 ns | 18.60 ns |  8.10 |    0.14 |    8 | 0.9537 | 0.0076 |    7992 B |       11.75 |
