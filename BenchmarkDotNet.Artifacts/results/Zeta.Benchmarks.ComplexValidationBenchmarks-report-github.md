```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a


```
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| Zeta_Valid               | 1.606 μs | 0.0043 μs | 0.0038 μs |  1.00 |    0.00 |    1 | 0.1125 |      - |     952 B |        1.00 |
| FluentValidation_Valid   | 1.778 μs | 0.0210 μs | 0.0197 μs |  1.11 |    0.01 |    2 | 0.5856 | 0.0019 |    4904 B |        5.15 |
| Zeta_Invalid             | 2.280 μs | 0.0360 μs | 0.0301 μs |  1.42 |    0.02 |    3 | 0.8163 | 0.0038 |    6856 B |        7.20 |
| FluentValidation_Invalid | 5.806 μs | 0.0408 μs | 0.0381 μs |  3.62 |    0.02 |    4 | 2.1667 | 0.0534 |   18136 B |       19.05 |
