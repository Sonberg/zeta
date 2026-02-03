```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.102
  [Host]   : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                          | Mean      | Error     | StdDev   | Gen0   | Allocated |
|-------------------------------- |----------:|----------:|---------:|-------:|----------:|
| ValidateStringWithMultipleRules | 100.92 ns |  6.460 ns | 0.354 ns |      - |         - |
| ValidateIntWithMinMax           |  36.49 ns | 14.741 ns | 0.808 ns | 0.0038 |      32 B |
| ValidateStringWithManyRules     | 139.38 ns | 10.839 ns | 0.594 ns |      - |         - |
