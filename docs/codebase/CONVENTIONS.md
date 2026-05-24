# CONVENTIONS.md — TermStructure Code Standards

## Naming Conventions

### Classes & Interfaces

| Category | Pattern | Example | Evidence |
|----------|---------|---------|----------|
| **Abstract Base Classes** | No prefix, `Abstract` suffix optional | `RateCurveBuilder` | Builders/ |
| **Concrete Implementations** | Descriptive name | `YieldCurveBuilder`, `LinearCurveBuildingStrategy` | Builders/, Strategies/ |
| **Interfaces** | `I` prefix | `ICurveBuildingStrategy`, `IInterpolationStrategy` | Strategies/ |
| **Data Models** | Descriptive noun | `YieldPoint`, `BondQuote`, `InterestRate` | Models/ |
| **Factory/Dispatcher** | `Dispatcher` suffix | `CurveBuilderDispatcher` | Builders/ |
| **Service Classes** | `[Domain]Fetcher` or `[Domain]Service` | `FredDataFetcher`, `MarketDataFetcher` | Services/ |

**Evidence**: File listing and class names in `TermStructure/` subdirectories

### Methods

| Category | Pattern | Example | Notes |
|----------|---------|---------|-------|
| **Public Methods** | PascalCase, verb-first | `Build()`, `GetRate()`, `Densify()`, `GetLatestValueAsync()` | Standard C# convention |
| **Async Methods** | `Async` suffix | `GetLatestValueAsync()`, `GetSeriesObservationsAsync()` | TAP (Task Async Pattern) |
| **Private Methods** | PascalCase (same as public) | `TransformPoints()`, `ParseTenor()` | Protected virtual methods are public-facing |
| **Factory Methods** | `Create` prefix | `CreateBuilder()` | CurveBuilderDispatcher |

**Evidence**: Method names in `FredDataFetcher.cs`, `RateCurveBuilder.cs`, `CurveBuilderDispatcher.cs`

### Variables & Fields

| Category | Pattern | Example |
|----------|---------|---------|
| **Local Variables** | camelCase | `rawPoints`, `transformedPoints`, `httpClient`, `seriesId` |
| **Parameters** | camelCase | `bonds`, `rates`, `maturity`, `step` |
| **Private Fields** | `_camelCase` (underscore prefix) | `_apiKey`, `BuildingStrategy` (property, not field) |
| **Properties** | PascalCase | `BuildingStrategy`, `InterpolationStrategy`, `MaturityYears` |
| **Constants** | UPPER_SNAKE_CASE | `BaseUrl`, `TODO_LIMIT` (in scan.py) |

**Evidence**: Variable names in `FredDataFetcher.cs`, test setup in `RateCurveBuilderTests.cs`

---

## C# Language Features

### Nullable Reference Types

**Status**: Enabled (`<Nullable>enable</Nullable>` in `.csproj`)

**Conventions**:
- Non-nullable by default: `public string Isin { get; set; }` cannot be null
- Explicit nullable: `public string? Tenor { get; set; }` can be null
- Constructor validation: `ArgumentNullException` thrown for null dependencies

**Evidence**: `.csproj` files, `FredDataFetcher` constructor validation

### Implicit Global Usings

**Status**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)

**Effect**: No `using System;`, `using System.Collections.Generic;` etc. required at top of files

**Common Implicit Namespaces**:
- `System`
- `System.Linq`
- `System.Threading.Tasks`
- `System.Collections.Generic`

**Evidence**: `.csproj` files; no `using` statements shown in source code

### Immutable Models

**Trend**: Data models are value-like (e.g., `YieldPoint` has only auto-properties)

```csharp
public class YieldPoint
{
    public decimal Maturity { get; set; }
    public decimal Rate { get; set; }
}
```

No constructors or complex logic — pure data holders.

**Evidence**: `Models/YieldPoint.cs`, `Models/BondQuote.cs`, `Models/InterestRate.cs`

---

## Formatting & Style

### Indentation

- **Tab Size**: 4 spaces (inferred from code samples)
- **Line Length**: [TODO] No `.editorconfig` file to specify

### Method Organization

**Typical Class Structure**:
1. Fields and Properties
2. Constructors
3. Public Methods (API surface)
4. Private / Protected Methods
5. Virtual Hooks (for template method pattern)

**Evidence**: `RateCurveBuilder.cs` (constructor first, then public methods, then protected `TransformPoints()`)

### Whitespace

- Blank lines between logical sections
- No trailing whitespace (inferred from clean builds)

**[TODO]** Specific whitespace rules not documented; inferred from code style

---

## Error Handling

### Null Checks

**Pattern**: Constructor validation with `ArgumentNullException`

```csharp
public FredDataFetcher(HttpClient httpClient, string apiKey) : base(httpClient)
{
    _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
}
```

**Evidence**: `FredDataFetcher.cs`, `RateCurveBuilder.cs` (constructor validation)

### Parsing & Conversion

**Pattern**: Use `*.TryParse()` for safe conversions; return nullable type

```csharp
bool status = decimal.TryParse(observations[0].Value, CultureInfo.InvariantCulture, out var value);
return status ? value : (decimal?)null;
```

**Evidence**: `FredDataFetcher.GetLatestValueAsync()` method

### Async Exception Handling

**Pattern**: Tests verify exception handling in async methods via xUnit/Moq

```csharp
[Fact]
public async Task GetLatestValueAsync_ReturnsNull_WhenNoObservations()
{
    // Arrange: mock response with no data
    // Act: call async method
    // Assert: verify null return
}
```

**Evidence**: `FredDataTests.cs` test cases

---

## Import Organization

### Using Statements

**Order** (inferred):
1. System namespaces (`using System;`)
2. System.* extension namespaces (`using System.Net;`, `using System.Text.Json;`)
3. Custom project namespaces (`using TermStructure.Models;`)
4. Third-party libraries (`using Moq;` in tests)

**Note**: Implicit global usings enabled, so many files may not show explicit `using` statements

**Evidence**: Top of `FredDataFetcher.cs`, `RateCurveBuilderTests.cs`

---

## Dependency Injection Conventions

### Constructor-Based DI

**Pattern**: All dependencies injected via constructor, no property setters

```csharp
public RateCurveBuilder(
    ICurveBuildingStrategy buildingStrategy,
    IInterpolationStrategy interpolationStrategy)
{
    BuildingStrategy = buildingStrategy ?? throw new ArgumentNullException(nameof(buildingStrategy));
    InterpolationStrategy = interpolationStrategy ?? throw new ArgumentNullException(nameof(interpolationStrategy));
}
```

**Validation**: Nulls rejected immediately (fail-fast)

**Evidence**: `RateCurveBuilder.cs` constructor

---

## Documentation Conventions

### XML Documentation Comments

**Status**: Enabled (xUnit analyzers check for documentation)

**Pattern** (observed in tests):
```csharp
/// <summary>
/// Retrieves the latest observation value for a FRED series.
/// </summary>
/// <param name="seriesId">The FRED series identifier (e.g., "DGS10")</param>
/// <returns>The parsed decimal value, or null if unavailable or unparseable</returns>
```

**[TODO]** Not all classes may have full XML docs; project recommends 100% documentation per guidelines

**Evidence**: Method signatures in `FredDataFetcher.cs`; `.github/instructions/csharp-solid-principles.instructions.md` recommends full documentation

### Test Documentation

**Pattern**: Descriptive test names follow `Method_Scenario_ExpectedResult`

```csharp
[Fact]
public void Build_WithValidBondsAndRates_ReturnsCurvePoints() { ... }

[Fact]
public async Task GetLatestValueAsync_ReturnsNull_WhenNoObservations() { ... }
```

**Evidence**: Test class names in `RateCurveBuilderTests.cs`, `FredDataTests`

---

## Async/Await Conventions

### Async Method Naming

- Suffix: `Async`
- Return Type: `Task<T>` for async operations
- Await Pattern: `await` used in callers

```csharp
public async Task<decimal?> GetLatestValueAsync(string seriesId)
{
    // async implementation
}

// In test:
var result = await fred.GetLatestValueAsync("DGS10");
```

**Evidence**: `FredDataFetcher.cs`, `FredDataTests.cs`

---

## Testing Conventions

### Naming

- Test Class: `[ComponentName]Tests` or `[ComponentName]Test`
- Test Method: `[MethodName]_[Scenario]_[ExpectedResult]`

### Arrange-Act-Assert (AAA)

All tests follow the AAA pattern:
```csharp
[Fact]
public void Build_WithValidBondsAndRates_ReturnsCurvePoints()
{
    // Arrange: set up test data and dependencies
    var bonds = new List<BondQuote> { ... };
    var builder = CreateYieldCurveBuilderWithMocks();
    
    // Act: call the method under test
    var result = builder.Build(bonds, rates);
    
    // Assert: verify the results
    Assert.NotNull(result);
    Assert.Equal(expected, result);
}
```

**Evidence**: All test methods in `RateCurveBuilderTests.cs`, `FredDataTests.cs`

### Mocking Strategy

- **Framework**: Moq
- **Pattern**: Setup mock to return expected values
- **Verification**: Verify mock was called with expected arguments

```csharp
var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
buildingStrategyMock.Setup(s => s.BuildCurve(It.IsAny<...>(), It.IsAny<...>()))
    .Returns(expectedCurvePoints);

// ... call code under test ...

buildingStrategyMock.Verify(s => s.BuildCurve(...), Times.Once);
```

**Evidence**: `RateCurveBuilderTests.cs` (26 tests using this pattern)

---

## Code Quality Standards

### Coverage Target

- **Overall**: 90% minimum
- **Builders/Strategies**: 95%+ (critical modules)
- **Services**: 85%+
- **Models**: 80%+ (data transfer objects)

**Evidence**: `quality-assurance` skill, README.md, CI/CD configuration

### Compiler Warnings

**Treated As Errors**: Nullable reference warnings (`CS8625`, `CS8618`, `CS8604`)

**Example Warnings Observed**:
- CS8625: Cannot convert null to non-nullable reference type
- CS8618: Non-nullable property must have value
- CS0219: Variable assigned but never used

**Evidence**: Latest test run output showing build succeeded despite warnings

### Static Analysis

**Tool**: xUnit analyzer (built-in)

**Example Rules**:
- xUnit2002: Do not use `Assert.NotNull()` on value types (detected in test run)

---

## Package Management Conventions

### NuGet Versions

**Approach**: Explicit pinned versions in `.csproj`
- `xunit` v2.9.2
- `Moq` v4.20.72
- `coverlet.collector` v6.0.2

**No Floating Versions**: All dependencies locked to specific versions

**Evidence**: `TermStructure.Tests.csproj` `<ItemGroup><PackageReference>` entries

---

## [TODO] / [ASK USER] Items

- [ ] **[TODO]** No `.editorconfig` file to specify line length, spacing, indentation
- [ ] **[ASK USER]** Should project adopt StyleCop (static analysis for style rules)?
- [ ] **[ASK USER]** Should floating patch versions be allowed (e.g., `4.20.*`)?
- [ ] **[TODO]** Code comments vs. self-documenting code preference not established

---

**Evidence Files**:
- `TermStructure/` class and method names
- `TermStructure.Tests/` test naming
- `.csproj` files (using statements, nullable config)
- `README.md` (SOLID principles section)
- `.github/instructions/csharp-solid-principles.instructions.md`
