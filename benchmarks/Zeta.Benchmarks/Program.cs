using BenchmarkDotNet.Running;

// Run all *Benchmarks in this assembly; pass --filter to pick one, e.g.
//   dotnet run -c Release -- --filter *ComplexValidationBenchmarks*
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
