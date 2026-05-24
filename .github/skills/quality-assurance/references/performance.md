# Performance Benchmarking Guide (BenchmarkDotNet)

## Overview

BenchmarkDotNet measures performance of critical code paths and detects regressions.

## Setup

### Create Benchmark Project (if not exists)

```bash
dotnet new classlib -n TermStructure.Benchmarks
dotnet add TermStructure.Benchmarks package BenchmarkDotNet
cd TermStructure.Benchmarks
```

### Create Benchmark Class

```csharp
using BenchmarkDotNet.Attributes;
using TermStructure.Builders;
using TermStructure.Models;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class CurveBuildingBenchmarks
{
    private List<YieldPoint> _points;
    private RateCurveBuilder _builder;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
        _builder = new RateCurveBuilder();
        _points = GenerateTestPoints(100);
    }

    [Benchmark]
    public void BuildCurve()
    {
        _builder.Build(_points);
    }

    [Benchmark]
    public void InterpolateCurve()
    {
        var curve = _builder.Build(_points);
        _ = curve.Interpolate(5.5);
    }
}
```

## Running Benchmarks

### Release Build (Required)
```bash
dotnet run --project TermStructure.Benchmarks --configuration Release
```

### Run Specific Benchmark
```bash
dotnet run --project TermStructure.Benchmarks --configuration Release -- --filter CurveBuildingBenchmarks
```

### Compare Against Baseline
```bash
dotnet run --project TermStructure.Benchmarks --configuration Release -- --baselines MyBaseline
```

## Interpreting Results

### Key Metrics

- **Mean**: Average execution time
- **StdDev**: Standard deviation (consistency)
- **Allocated**: Memory allocated (Gen 0/1/2)
- **Ratio**: Comparison to baseline (1.0 = no change)

### Example Output

```
|           Method |     Mean |   StdDev |  Ratio | Gen0 | Gen1 | Gen2 | Allocated |
|----------------- |---------:|--------:|--------:|-----:|-----:|-----:|-----------:|
| BuildCurve       |  42.5 us | 2.1 us  | 1.00 | 10   |    0 |    0 |   8 KB    |
| InterpolateCurve |   1.2 us | 0.1 us  | 1.00 |  0   |    0 |    0 |  0 KB    |
```

## Performance Targets

- **Curve building** (100 points): < 100ms
- **Single interpolation**: < 1ms
- **Memory allocation**: < 10KB per operation
- **Regression threshold**: < 10% slowdown acceptable

## Detecting Regressions

1. **Run baseline benchmark**:
   ```bash
   dotnet run --project TermStructure.Benchmarks --configuration Release --save MyBaseline
   ```

2. **After code changes, run again**:
   ```bash
   dotnet run --project TermStructure.Benchmarks --configuration Release
   ```

3. **Compare results** — If Ratio > 1.10, investigate

## Optimization Checklist

- [ ] Profile hot paths with dotnet-trace
- [ ] Check for unnecessary allocations (Gen0 growth)
- [ ] Verify algorithm complexity (O(n), O(n²), etc.)
- [ ] Consider caching for repeated operations
- [ ] Use ValueTypes for small data (struct vs class)
- [ ] Benchmark in Release, not Debug

## Reports

Results stored in: `BenchmarkDotNet.Artifacts/results/`

View HTML report:
```bash
open BenchmarkDotNet.Artifacts/results/summary.html
```
