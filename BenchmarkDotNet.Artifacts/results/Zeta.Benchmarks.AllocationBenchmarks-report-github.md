```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]   : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                          | Mean      | Error    | StdDev   | Gen0   | Allocated |
|-------------------------------- |----------:|---------:|---------:|-------:|----------:|
| ValidateStringWithMultipleRules |  98.72 ns | 4.384 ns | 0.240 ns |      - |         - |
| ValidateIntWithMinMax           |  39.17 ns | 2.002 ns | 0.110 ns | 0.0038 |      32 B |
| ValidateStringWithManyRules     | 128.87 ns | 7.345 ns | 0.403 ns |      - |         - |
