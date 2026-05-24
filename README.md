# TermStructure

Build and interpolate interest rate term structures from bond quotes and market data.

[![Code Coverage](https://img.shields.io/badge/coverage-90%25-brightgreen)](./coverage)
[![Tests](https://img.shields.io/badge/tests-31%20passing-brightgreen)](./TermStructure.Tests)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com)

## Overview

**TermStructure** is a C# library for constructing and interpolating interest rate term structures (yield curves) from bond quotes and market rates. It provides pluggable strategies for different curve building methodologies and interpolation techniques.

The project implements modern C# design patterns (Strategy, Builder, Dependency Injection) with comprehensive testing and SOLID principles adherence. It's designed for financial applications requiring accurate yield curve analysis and interpolation.

### Key Features

- **Pluggable Curve Building Strategies**: Bootstrap, Linear, and Nelson-Siegel methods
- **Multiple Interpolation Strategies**: Linear interpolation and extensible design
- **Type-Safe Data Models**: Strong typing for bonds, rates, and yield points
- **External Data Integration**: FRED (Federal Reserve Economic Data) API integration
- **Comprehensive Testing**: 31 unit tests with 90%+ code coverage
- **SOLID Principles**: Clean architecture following Single Responsibility, Interface Segregation, and Dependency Inversion

## Architecture

### Design Patterns

- **Strategy Pattern**: `ICurveBuildingStrategy` and `IInterpolationStrategy` allow swapping algorithms at runtime
- **Builder Pattern**: `RateCurveBuilder` and `YieldCurveBuilder` provide fluent curve construction
- **Template Method Pattern**: Base `RateCurveBuilder` defines the algorithm skeleton, subclasses override specific steps
- **Dependency Injection**: Constructor-based DI for testability and loose coupling

### Module Structure

```
TermStructure/
├── Builders/          (3 classes)
│   ├── RateCurveBuilder          (abstract base, template method)
│   ├── YieldCurveBuilder         (yield curve specialization)
│   └── CurveBuilderDispatcher    (factory for selecting builders)
├── Services/          (2 classes)
│   ├── FredDataFetcher           (external API integration)
│   └── MarketDataFetcher         (abstract base)
├── Strategies/        (6 interfaces + implementations)
│   ├── BuildingStrategies/
│   │   ├── ICurveBuildingStrategy
│   │   ├── BootstrapCurveBuildingStrategy
│   │   ├── LinearCurveBuildingStrategy
│   │   └── NelsonSiegelCurveBuildingStrategy
│   └── InterpolationStrategies/
│       ├── IInterpolationStrategy
│       └── LinearInterpolationStrategy
├── Models/            (5 data classes)
│   ├── YieldPoint
│   ├── BondQuote
│   ├── InterestRate
│   ├── FredObservation
│   └── FredSeriesResponse
└── Tests/             (31 tests, 90%+ coverage)
    ├── FredDataTests          (5 async tests)
    └── RateCurveBuilderTests  (26 comprehensive tests)
```

### Dependency Flow

```
Application
  ↓
CurveBuilderDispatcher (factory)
  ↓
RateCurveBuilder + YieldCurveBuilder
  ↓
ICurveBuildingStrategy    +    IInterpolationStrategy
  ↓                             ↓
[Bootstrap, Linear,        [Linear]
 Nelson-Siegel]
  ↓
Models (YieldPoint, BondQuote, InterestRate)
```

**Key Dependencies**:
- `FredDataFetcher` → `MarketDataFetcher` (abstract base)
- `RateCurveBuilder` → `ICurveBuildingStrategy`, `IInterpolationStrategy`
- Tests use `Moq` for strategy mocking

## Installation & Setup

### Prerequisites

- **.NET 9.0 SDK** or later
- **Git** for cloning the repository
- **Visual Studio Code** or **Visual Studio 2022** (optional, for IDE support)

### Building

```bash
# Clone the repository
git clone https://github.com/thebeast/IRCurves.git
cd IRCurves

# Build in Release configuration
dotnet build --configuration Release

# Output: TermStructure/bin/Release/net9.0/TermStructure.dll
```

### Running Tests

```bash
# Run all tests
dotnet test TermStructure.Tests --configuration Release

# Run with coverage
dotnet test TermStructure.Tests /p:CollectCoverage=true /p:CoverageThreshold=90

# Run specific test class
dotnet test --filter "ClassName=RateCurveBuilderTests"
```

### Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| **xUnit.net** | 2.x | Unit testing framework |
| **Moq** | 4.20+ | Mocking library for unit tests |
| **Coverlet** | Latest | Code coverage measurement |
| **System.Net.Http** | Built-in | HTTP client for FRED API |
| **System.Text.Json** | Built-in | JSON deserialization |

## Usage Examples

### Basic: Build a Yield Curve

```csharp
using TermStructure;
using TermStructure.Builders;
using TermStructure.Strategies.BuildingStrategies;
using TermStructure.Strategies.InterpolationStrategies;
using TermStructure.Models;

// Create bond quotes (market data)
var bonds = new List<BondQuote>
{
    new BondQuote
    {
        BondId = "BOND001",
        MaturityYears = 1m,
        YieldToMaturity = 0.0425m,
        CouponRate = 0.04m
    },
    new BondQuote
    {
        BondId = "BOND002",
        MaturityYears = 10m,
        YieldToMaturity = 0.0500m,
        CouponRate = 0.045m
    }
};

var rates = new List<InterestRate>
{
    new InterestRate { Currency = "USD", RateType = "LIBOR", Rate = 0.0425m, Tenor = "1Y" },
    new InterestRate { Currency = "USD", RateType = "LIBOR", Rate = 0.0500m, Tenor = "10Y" }
};

// Create builder with strategies
var builder = new YieldCurveBuilder(
    new LinearCurveBuildingStrategy(),
    new LinearInterpolationStrategy()
);

// Build the curve
var curve = builder.Build(bonds, rates);
// Result: List<YieldPoint> with interpolated yield points
```

### Intermediate: Get Interpolated Rate

```csharp
// Get the yield at a specific maturity (e.g., 5.5 years)
decimal rate = builder.GetRate(5.5m, bonds, rates);
// Result: 0.0463m (interpolated between 1Y and 10Y)
```

### Advanced: Densify the Curve

```csharp
// Create a dense curve with points every 0.5 years
var denseCurve = builder.Densify(0.5m, bonds, rates);
// Result: YieldPoint for 1.0, 1.5, 2.0, ..., 10.0 years
// Useful for visualization and numerical analysis
```

### Working with FRED API

```csharp
using TermStructure.Services;

var httpClient = new HttpClient();
var fredFetcher = new FredDataFetcher(httpClient, "YOUR_FRED_API_KEY");

// Fetch the latest 10-year Treasury yield
var dgs10Rate = await fredFetcher.GetLatestValueAsync("DGS10");
// Result: 0.042m (example)

// Get multiple observations
var observations = await fredFetcher.GetSeriesObservationsAsync("DGS10", "desc", 30);
// Result: Last 30 observations sorted descending
```

## API Reference

### Core Classes

#### RateCurveBuilder (Abstract)

Base class for building interest rate curves using strategies.

```csharp
public abstract class RateCurveBuilder
{
    // Build a curve from bonds and rates
    public List<YieldPoint> Build(IEnumerable<BondQuote> bonds, IEnumerable<InterestRate> rates);
    
    // Get interpolated rate at specific maturity
    public decimal GetRate(decimal maturity, IEnumerable<BondQuote> bonds, IEnumerable<InterestRate> rates);
    
    // Densify curve to specified step
    public List<YieldPoint> Densify(decimal step, IEnumerable<BondQuote> bonds, IEnumerable<InterestRate> rates);
    
    // Virtual method for subclass transformations
    protected virtual List<YieldPoint> TransformPoints(List<YieldPoint> rawPoints);
}
```

#### YieldCurveBuilder

Specialization of `RateCurveBuilder` for yield curves.

```csharp
public class YieldCurveBuilder : RateCurveBuilder
{
    public YieldCurveBuilder(ICurveBuildingStrategy buildingStrategy, IInterpolationStrategy interpolationStrategy);
}
```

#### FredDataFetcher

Fetches economic data from the FRED API.

```csharp
public class FredDataFetcher : MarketDataFetcher
{
    public FredDataFetcher(HttpClient httpClient, string apiKey);
    
    // Get all observations for a series
    public async Task<List<FredObservation>> GetSeriesObservationsAsync(string seriesId, string sortOrder = "desc", int limit = 0);
    
    // Get latest observation value
    public async Task<decimal?> GetLatestValueAsync(string seriesId);
}
```

### Data Models

```csharp
public class YieldPoint
{
    public decimal Maturity { get; set; }
    public decimal Rate { get; set; }
}

public class BondQuote
{
    public string BondId { get; set; }
    public string Isin { get; set; }
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
    public decimal YieldToMaturity { get; set; }
    public decimal MaturityYears { get; set; }
    public decimal CouponRate { get; set; }
    public int CouponFrequency { get; set; }
}

public class InterestRate
{
    public required string Currency { get; set; }
    public required string RateType { get; set; }
    public decimal Rate { get; set; }
    public string? Tenor { get; set; }
}
```

### Strategy Interfaces

```csharp
public interface ICurveBuildingStrategy
{
    List<YieldPoint> BuildCurve(IEnumerable<BondQuote> bonds, IEnumerable<InterestRate> rates);
}

public interface IInterpolationStrategy
{
    // Interpolate rate at given maturity
    decimal Interpolate(decimal x, List<YieldPoint> points);
    
    // Create dense curve
    List<YieldPoint> Densify(List<YieldPoint> points, decimal step);
}
```

## Testing & Quality

### Test Framework

- **Framework**: xUnit.net (modern .NET testing)
- **Mocking**: Moq 4.20+ (for strategy mocking)
- **Coverage Tool**: Coverlet (OpenCover format)
- **Target Coverage**: 90% minimum, 95%+ for critical modules

### Test Organization

**FredDataTests** (5 tests)
- `GetLatestValueAsync_ReturnsCorrectDecimal` — Happy path
- `GetLatestValueAsync_ReturnsNull_WhenNoObservations` — Edge case
- `GetLatestValueAsync_HandlesDotValue` — Error handling
- `GetSeriesObservationsAsync_ReturnsAllObservations` — Data retrieval
- `GetSeriesObservationsAsync_RespectsSortOrderAndLimit` — Parameter handling

**RateCurveBuilderTests** (26 tests)
- Happy paths: Valid input, single bond, real strategies
- Edge cases: Empty collections, large datasets (100+ points), zero/negative maturities
- Error handling: Null dependencies, invalid inputs
- Integration: Multi-build consistency, template method pattern
- Densification: Various step sizes, interpolation verification

### Running Tests

```bash
# All tests
dotnet test TermStructure.Tests --configuration Release

# With coverage measurement
dotnet test TermStructure.Tests /p:CollectCoverage=true /p:CoverageFormat=opencover

# Specific test class
dotnet test --filter "ClassName=RateCurveBuilderTests"

# Specific test
dotnet test --filter "Name~GetLatestValueAsync_ReturnsCorrectDecimal"
```

**Current Metrics**:
- ✅ Total Tests: 31
- ✅ Pass Rate: 100%
- ✅ Coverage Target: 90%
- ✅ Builders/Strategies: 95%+ coverage
- ✅ Services: 85%+ coverage

## Contributing

### Guidelines

1. **Fork the repository** and create a feature branch (`git checkout -b feature/my-feature`)
2. **Write tests first** (TDD) or comprehensive tests with new code
3. **Ensure 90%+ coverage** for new/modified code
4. **Follow SOLID principles** — See [SOLID Principles Guide](.github/instructions/csharp-solid-principles.instructions.md)
5. **Update documentation** when adding features or APIs
6. **Submit a PR** with clear description of changes

### Development Workflow

```bash
# Create feature branch
git checkout -b feature/add-nelson-siegel

# Make changes and run tests
dotnet test TermStructure.Tests --configuration Release

# Check coverage
dotnet test /p:CollectCoverage=true /p:CoverageThreshold=90

# Commit with descriptive message
git commit -m "feat: implement Nelson-Siegel curve building strategy

- Adds NelsonSiegelCurveBuildingStrategy class
- Implements 4-factor Nelson-Siegel model
- Includes 8 unit tests with edge cases
- Coverage: 96% for strategy module"

# Push and create PR
git push origin feature/add-nelson-siegel
```

### Code Review Checklist

- [ ] All tests pass (`dotnet test`)
- [ ] Code coverage ≥ 90% (`/p:CollectCoverage=true`)
- [ ] SOLID principles followed (SRP, ISP, DIP)
- [ ] Public APIs have XML documentation (`/// <summary>`)
- [ ] No breaking changes (or documented)
- [ ] README updated if new features added
- [ ] Follows existing code style and patterns

### Code Standards

- **SOLID Principles**: See [SOLID for C#](.github/instructions/csharp-solid-principles.instructions.md)
- **Testing**: See [Testing Guide](.github/skills/quality-assurance/references/testing.md)
- **Coverage**: See [Coverage Guide](.github/skills/quality-assurance/references/coverage.md)

## Documentation

- [SOLID Principles for C#](.github/instructions/csharp-solid-principles.instructions.md) — Code design principles
- [Quality Assurance](.github/skills/quality-assurance/SKILL.md) — Testing and coverage workflow
- [Test Expert Agent](.github/agents/test-expert.agent.md) — Test writing assistance
- [Project Documentation Skill](.github/skills/project-documentation/SKILL.md) — README management

## Version History

- **v1.0.0** (2026-05-23) — Initial release with 3 curve building strategies, linear interpolation, FRED API integration, 31 tests

## License

[Add your license here]

## Support & Contact

- **Issues**: Report bugs or feature requests in the [GitHub Issues](https://github.com/thebeast/IRCurves/issues)
- **Discussions**: Ask questions in [GitHub Discussions](https://github.com/thebeast/IRCurves/discussions)
- **Maintainer**: [Your Name/Email]

---

**Last Updated**: 2026-05-24
**Documentation Version**: 1.0.0
**Status**: ✅ All checks passing (31/31 tests, 90%+ coverage)
