# ARCHITECTURE.md — TermStructure System Design

## Architectural Overview

**TermStructure** follows a **layered, strategy-based architecture** for building and interpolating interest rate term structures (yield curves).

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│         Client Application Layer                    │
│  (Console, API, Financial App, etc.)               │
└─────────────────────────────┬───────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────┐
│     CurveBuilderDispatcher (Factory)               │
│  Selects appropriate builder based on curve type  │
└─────────────────────────────┬───────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────┐
│    RateCurveBuilder (Template Method)              │
│  1. Call ICurveBuildingStrategy.BuildCurve()      │
│  2. Call TransformPoints() (hook for subclasses)  │
│  3. Return transformed yield points               │
└─────────────────────────────┬───────────────────────┘
                              │
         ┌────────────────────┼────────────────────┐
         │                    │                    │
    ┌────▼────────────┐  ┌────▼──────────────┐   │
    │ YieldCurveBuilder│  │InterestRateModel  │   │
    │ (Default: no    │  │   Methods         │   │
    │  transformation) │  └──────────────────┘   │
    └─────────────────┘                          │
         │                                        │
         │                    ┌───────────────────▼────────────┐
         │                    │ Strategies (pluggable)        │
         │                    │                               │
         │    ┌───────────────┼───────────────┐               │
         │    │               │               │               │
    ┌────▼────▼────┐  ┌───────▼──────┐  ┌────▼────────┐     │
    │ ICurveBuilding│  │IInterpolation│  │ Integration │     │
    │Strategy       │  │Strategy      │  │ (FRED API)  │     │
    │               │  │              │  │             │     │
    │• Bootstrap    │  │• Linear      │  │FredDataFetch│     │
    │• Linear       │  │              │  │er (async)   │     │
    │• Nelson-Siegel│  │              │  │             │     │
    └────────────┬──┘  └──────────────┘  └─────────────┘     │
                 │                                             │
                 └─────────────────────────────────────────────┘
                              │
         ┌────────────────────▼───────────────────┐
         │  Models (Data Transfer Objects)       │
         │                                        │
         │• YieldPoint (maturity, rate)          │
         │• BondQuote (market snapshot)          │
         │• InterestRate (tenor + type)          │
         │• FredObservation (time series point) │
         └────────────────────────────────────────┘
```

---

## Design Patterns

### 1. Strategy Pattern (`ICurveBuildingStrategy`, `IInterpolationStrategy`)

**Purpose**: Allow swapping curve building and interpolation algorithms at runtime.

**Implementation**:
```
┌──────────────────────────────────────┐
│ ICurveBuildingStrategy               │
│ + BuildCurve(bonds, rates)           │
└──────────────────┬───────────────────┘
                   │
      ┌────────────┼────────────┬──────────────┐
      │            │            │              │
┌─────▼──────────┐ │ ┌──────────▼──────┐ ┌────▼──────────┐
│ Bootstrap      │ │ │Linear           │ │Nelson-Siegel  │
│ Strategy       │ │ │Strategy         │ │Strategy       │
└────────────────┘ │ └─────────────────┘ └───────────────┘
                   │
                Same for IInterpolationStrategy
                (currently only LinearInterpolationStrategy)
```

**Evidence**: `TermStructure/Strategies/BuildingStrategies/ICurveBuildingStrategy.cs` and implementations

### 2. Template Method Pattern (`RateCurveBuilder`)

**Purpose**: Define the skeleton of curve building algorithm; let subclasses override specific steps.

**Implementation**:
```csharp
public abstract class RateCurveBuilder
{
    // Template method (final algorithm)
    public List<YieldPoint> Build(IEnumerable<BondQuote> bonds, IEnumerable<InterestRate> rates)
    {
        var rawPoints = BuildingStrategy.BuildCurve(bonds, rates);
        var transformedPoints = TransformPoints(rawPoints);  // Hook: override in subclass
        return transformedPoints;
    }
    
    // Hook for subclasses (default: identity transformation)
    protected virtual List<YieldPoint> TransformPoints(List<YieldPoint> rawPoints)
    {
        return rawPoints;
    }
}

// Subclass (can override TransformPoints for forward curves, etc.)
public class YieldCurveBuilder : RateCurveBuilder
{
    // Inherits template method, uses default transformation (no change)
}
```

**Evidence**: `TermStructure/Builders/RateCurveBuilder.cs` and `YieldCurveBuilder.cs`

### 3. Builder Pattern (`CurveBuilderDispatcher`)

**Purpose**: Factory for creating appropriate curve builders based on curve type.

**Implementation**:
```csharp
public class CurveBuilderDispatcher
{
    public RateCurveBuilder CreateBuilder(
        CurveType type,
        ICurveBuildingStrategy building,
        IInterpolationStrategy interpolation)
    {
        return type switch
        {
            CurveType.Yield => new YieldCurveBuilder(building, interpolation),
            // ... other types
        };
    }
}
```

**Evidence**: `TermStructure/Builders/CurveBuilderDispatcher.cs`

### 4. Dependency Injection (Constructor-based)

**Purpose**: Decouple from concrete implementations; enable testing with mocks.

**Implementation**:
```csharp
// Inject strategies into builder
var builder = new YieldCurveBuilder(
    new LinearCurveBuildingStrategy(),      // Injected
    new LinearInterpolationStrategy()       // Injected
);

// Testing: inject mocks
var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
var builder = new YieldCurveBuilder(
    buildingStrategyMock.Object,            // Mock injected
    new LinearInterpolationStrategy()
);
```

**Evidence**: Constructor signatures in `RateCurveBuilder`, test fixtures in `RateCurveBuilderTests.cs`

---

## Layered Architecture

| Layer | Responsibility | Examples |
|-------|---|---|
| **Presentation/API** | Client-facing interface | `CurveBuilderDispatcher` (factory interface) |
| **Application** | Orchestration, workflow | `RateCurveBuilder`, `YieldCurveBuilder` |
| **Domain** | Business logic, algorithms | `ICurveBuildingStrategy` implementations (Bootstrap, Linear, Nelson-Siegel) |
| **Infrastructure** | External service integration | `FredDataFetcher` (async REST client) |
| **Data** | Models and DTOs | `YieldPoint`, `BondQuote`, `InterestRate` |

---

## Data Flow

### Scenario: Build a Yield Curve

```
Input: List<BondQuote> + List<InterestRate>
  │
  ├─→ CurveBuilderDispatcher.CreateBuilder()
  │   (returns YieldCurveBuilder with selected strategies)
  │
  ├─→ YieldCurveBuilder.Build(bonds, rates)
  │   │
  │   ├─→ ICurveBuildingStrategy.BuildCurve(bonds, rates)
  │   │   (e.g., LinearCurveBuildingStrategy)
  │   │   Returns: List<YieldPoint>
  │   │
  │   └─→ TransformPoints(rawPoints)
  │       (default: returns raw points as-is)
  │
  └─→ Output: List<YieldPoint> (interpolated curve)
```

### Scenario: Fetch External Market Data

```
Input: FRED Series ID (e.g., "DGS10")
  │
  ├─→ FredDataFetcher.GetLatestValueAsync(seriesId)
  │   (async HTTP GET to api.stlouisfed.org/fred/series/observations)
  │
  ├─→ Parse JSON response (System.Text.Json)
  │   Convert to List<FredObservation>
  │
  └─→ Output: decimal? (parsed rate or null if failed)
```

---

## Key Responsibilities by Module

### `Builders/`
- **RateCurveBuilder**: Template method for curve building
- **YieldCurveBuilder**: Yield curve specialization (identity transformation)
- **CurveBuilderDispatcher**: Factory for selecting builders

### `Strategies/`
- **BuildingStrategies**: Algorithms for constructing yield points from bonds/rates
  - Bootstrap: Iterative fitting
  - Linear: Direct linear mapping
  - Nelson-Siegel: 4-factor parametric model
- **InterpolationStrategies**: Algorithms for estimating rates at arbitrary maturities
  - Linear: Simple linear interpolation between points

### `Services/`
- **MarketDataFetcher**: Abstract base for market data retrieval
- **FredDataFetcher**: Concrete implementation (FRED API async HTTP client)

### `Models/`
- **YieldPoint**: (Maturity, Rate) tuple
- **BondQuote**: Bond market data snapshot
- **InterestRate**: Rate with tenor and type metadata
- **FredObservation**: FRED time series observation
- **FredSeriesResponse**: FRED API response wrapper

---

## SOLID Principles Adherence

| Principle | Implementation |
|-----------|---|
| **Single Responsibility** | Each class has one reason to change (e.g., `FredDataFetcher` only handles FRED API calls) |
| **Open/Closed** | Open for extension (new strategies), closed for modification (interfaces stable) |
| **Liskov Substitution** | `YieldCurveBuilder` can be swapped for any `RateCurveBuilder` without breaking code |
| **Interface Segregation** | Clients depend on `ICurveBuildingStrategy` or `IInterpolationStrategy`, not fat interfaces |
| **Dependency Inversion** | High-level modules depend on interfaces, not concrete classes (e.g., `RateCurveBuilder` depends on `ICurveBuildingStrategy`) |

**Evidence**: `.github/instructions/csharp-solid-principles.instructions.md` (project standard)

---

## Concurrency & Async

- **FredDataFetcher**: Async HTTP operations (`async Task<T>`)
- **RateCurveBuilder**: Synchronous operations (no async/await internally)
- **Tests**: 5 FredData tests are async; 26 RateCurveBuilder tests are synchronous

**Evidence**: `FredDataFetcher.cs` uses `async Task<>` methods; test class structure in `UnitTest1.cs` and `RateCurveBuilderTests.cs`

---

## Extensibility Points

1. **New Curve Building Strategies**: Implement `ICurveBuildingStrategy`
2. **New Interpolation Strategies**: Implement `IInterpolationStrategy`
3. **New Curve Types**: Extend `RateCurveBuilder` and add to `CurveBuilderDispatcher`
4. **New Market Data Sources**: Extend `MarketDataFetcher` (alongside `FredDataFetcher`)

---

## [TODO] / [ASK USER] Items

- [ ] **[TODO]** Forward curve builder (inherits `RateCurveBuilder`, overrides `TransformPoints()`)
- [ ] **[TODO]** Other interpolation strategies (cubic spline, etc.)
- [ ] **[ASK USER]** Should curve caching layer be added? Current design recalculates on each call.
- [ ] **[ASK USER]** Should multi-threaded curve fitting strategies be supported?

---

**Evidence Files**:
- `TermStructure/Builders/RateCurveBuilder.cs`
- `TermStructure/Builders/YieldCurveBuilder.cs`
- `TermStructure/Strategies/BuildingStrategies/ICurveBuildingStrategy.cs`
- `TermStructure/Strategies/InterpolationStrategies/IInterpolationStrategy.cs`
- `TermStructure/Services/FredDataFetcher.cs`
- README.md (Architecture section)
