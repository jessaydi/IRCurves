# BDD Test Suite for TermStructure Project

## Overview

This document describes the comprehensive Behavior-Driven Development (BDD) test suite for the TermStructure interest rate curve building project. The suite uses **SpecFlow/Gherkin** to define executable specifications that bridge business stakeholders and development teams.

## Test Artifacts

### Feature Files

Feature files describe business processes in human-readable Gherkin syntax:

#### 1. **CurveBuilding.feature**
Tests the core curve construction functionality with different strategies.

**Scope:**
- ✅ Bootstrap strategy for zero-coupon curve extraction
- ✅ Linear strategy for simple interest rate curves
- ✅ Nelson-Siegel strategy for smooth curve fitting
- ✅ Interpolation at arbitrary maturities
- ✅ Curve densification (sparse to dense point conversion)
- ✅ Scenario outlines with multiple tenor combinations

**Example Scenarios:**
```gherkin
Scenario: Build a yield curve using bootstrap strategy with valid bonds
  Given I have bond quotes with the following details:
    | BondId  | MaturityYears | YieldToMaturity | AskPrice | CouponRate |
    | BOND001 | 1             | 0.0425          | 99.75    | 0.04       |
  When I build the yield curve from these bonds
  Then the curve should contain 3 yield points
  And the curve should be sorted by maturity in ascending order
```

#### 2. **DataIntegration.feature**
Tests integration with the FRED API for real-world market data.

**Scope:**
- ✅ Fetching latest interest rate observations from FRED
- ✅ Bulk observation retrieval with sorting
- ✅ Conversion of FRED data to InterestRate model
- ✅ Building curves from FRED treasury series
- ✅ Data quality validation and error handling
- ✅ Caching to reduce API calls

**Example Scenarios:**
```gherkin
Scenario: Build yield curve from multiple FRED treasury series
  Given I fetch data from the following FRED treasury series:
    | SeriesId | ExpectedTenor |
    | DGS1     | 1Y            |
    | DGS5     | 5Y            |
    | DGS10    | 10Y           |
  When I aggregate the observations into interest rate data
  And I build a yield curve using the linear strategy
  Then the curve should contain 3 points
  And the curve should represent the current treasury yield curve shape
```

#### 3. **BusinessWorkflows.feature**
Tests end-to-end business processes and use cases.

**Scope:**
- ✅ Complete bootstrap workflow from bonds to usable curve
- ✅ Yield curve shape analysis (normal vs inverted)
- ✅ Bond pricing using constructed curves
- ✅ Comparative analysis across instruments
- ✅ Curve densification for reporting
- ✅ Scenario analysis (parallel shifts, flattening)
- ✅ Multi-period analysis for pattern detection

**Example Scenarios:**
```gherkin
Scenario: Complete workflow - Bootstrap curve from bond data
  Given I have a portfolio of bonds with varying maturities:
    | BondId  | MaturityYears | AskPrice |
    | BOND001 | 1             | 99.75    |
    | BOND002 | 5             | 98.50    |
  When I bootstrap the zero-coupon yield curve
  Then the yields at intermediate points should be monotonically increasing
  And I can use these rates for instrument pricing
```

#### 4. **EdgeCasesAndErrors.feature**
Tests error handling and boundary conditions.

**Scope:**
- ✅ Empty input collections
- ✅ Invalid/negative values
- ✅ Missing required fields (tenor)
- ✅ Malformed data formats
- ✅ Insufficient data for algorithms (Nelson-Siegel needs ≥3 points)
- ✅ Interpolation boundaries (extrapolation not allowed)
- ✅ FRED API errors (HTTP 503, malformed JSON)
- ✅ Decimal parsing failures
- ✅ Thread safety and race conditions

**Example Scenarios:**
```gherkin
Scenario: Handle empty bond collection
  Given I provide an empty list of bonds
  When I attempt to build a curve
  Then the system should return an empty yield point list
  And no exception should be thrown

Scenario: Handle FRED API HTTP errors
  Given the FRED API is temporarily unavailable (HTTP 503)
  When I attempt to fetch data from FRED
  Then the system should catch the HttpRequestException
  And return null for the requested data
```

## Step Definition Files

Step definitions implement the Gherkin steps in C#. Each file handles a specific domain:

### 1. **CurveBuildingSteps.cs** (Main logic)

**Responsibility:** Implement curve building workflow steps

**Key Methods:**
- `GivenIHaveBondQuotesWithDetails()` - Populate bond test data from Gherkin tables
- `GivenIUseBootstrapStrategy()` - Configure bootstrap strategy
- `GivenIUseLinearStrategy()` - Configure linear strategy
- `GivenIUseNelsonSiegelStrategyWithLambda()` - Configure Nelson-Siegel with lambda parameter
- `WhenIBuildCurveFromBonds()` - Execute curve building
- `WhenIRequestInterpolatedYieldAtDecimalMaturity()` - Interpolate at arbitrary maturity
- `WhenIDensifyCurveWithStep()` - Create dense curve from sparse points

**Example Implementation Pattern:**
```csharp
[Given(@"I have bond quotes with the following details:")]
public void GivenIHaveBondQuotesWithDetails(DataTable dataTable)
{
    _bondQuotes = dataTable.CreateSet<BondQuote>().ToList();
    _context["BondQuotes"] = _bondQuotes;
}

[When(@"I build the yield curve from these bonds")]
public void WhenIBuildCurveFromBonds()
{
    var rates = _context.ContainsKey("InterestRates") 
        ? (List<InterestRate>)_context["InterestRates"] 
        : new List<InterestRate>();

    _curveBuilder ??= new YieldCurveBuilder(
        new BootstrapCurveBuildingStrategy(),
        _interpolationStrategy ?? new LinearInterpolationStrategy()
    );

    _currentCurve = _curveBuilder.Build(_bondQuotes, rates);
    _context["CurrentCurve"] = _currentCurve;
}
```

### 2. **CurveValidationSteps.cs** (Assertions)

**Responsibility:** Verify curve properties and expected outcomes

**Key Methods:**
- `ThenCurveShouldContainPoints()` - Assert point count
- `ThenCurveShouldBeSortedByMaturity()` - Verify ascending maturity order
- `ThenAllYieldsShouldBePositive()` - Validate rate values
- `ThenInterpolatedRateShouldBeApproximately()` - Check interpolation accuracy
- `ThenCurveIsUpwardSloping()` - Identify normal yield curve
- `ThenSystemDetectsInvertedCurve()` - Identify inverted curve

**Example:**
```csharp
[Then(@"the curve should contain (\d+) yield points")]
public void ThenCurveShouldContainPoints(int expectedCount)
{
    var curve = (List<YieldPoint>)_context["CurrentCurve"];
    Assert.NotNull(curve);
    Assert.Equal(expectedCount, curve.Count);
}

[Then(@"the interpolated rate should be approximately (.*)")]
public void ThenInterpolatedRateShouldBeApproximately(string expectedStr)
{
    if (decimal.TryParse(expectedStr, out var expected))
    {
        var actual = (decimal)_context["InterpolatedRate"];
        Assert.True(Math.Abs(actual - expected) < 0.0001m);
    }
}
```

### 3. **DataFetchingSteps.cs** (FRED API)

**Responsibility:** Mock FRED API interactions and data transformation

**Key Methods:**
- `GivenFredApiConfigured()` - Initialize mocked HTTP client
- `GivenFredHasDataForSeries()` - Simulate API response with test data
- `WhenIRequestLatestValueAsync()` - Call GetLatestValueAsync
- `WhenIRequestSeriesObservationsAsync()` - Call GetSeriesObservationsAsync
- `WhenIAggregateObservations()` - Convert FRED data to InterestRate objects
- `ThenReturnedValueApproximate()` - Verify FRED value conversion

**Example:**
```csharp
[Given(@"the FRED service has data for series ""(.*)"" \((.*)\)")]
public void GivenFredHasDataForSeries(string seriesId, string description)
{
    var response = CreateFredSeriesResponse(seriesId, new[] { 3.25m });
    var responseMessage = new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(response, Encoding.UTF8, "application/json")
    };

    _fakeHandler = new FakeHttpHandler(responseMessage);
    _httpClient = new HttpClient(_fakeHandler);
    _fredFetcher = new FredDataFetcher(_httpClient, "test-api-key");
}

[When(@"I request the latest value for series (.*)")]
public async Task WhenIRequestLatestValueAsync(string seriesId)
{
    _latestValue = await _fredFetcher.GetLatestValueAsync(seriesId);
    _context["LatestValue"] = _latestValue;
}
```

### 4. **ErrorHandlingSteps.cs** (Validation)

**Responsibility:** Verify error conditions are handled gracefully

**Key Methods:**
- `ThenReturnsEmptyList()` - Assert empty list returned for invalid input
- `ThenCompletesWithoutError()` - Verify no exceptions thrown
- `ThenRaisesFormatException()` - Verify expected exception type
- `ThenNumericalStability()` - Check for NaN/Infinity values
- `ThenSystemOperational()` - Verify application didn't crash

### 5. **CurveBuildingHooks.cs** (Lifecycle)

**Responsibility:** Setup/teardown and cross-cutting concerns

**Key Hooks:**
- `[BeforeScenario]` - Initialize test context before each scenario
- `[AfterScenario]` - Clean up resources after each scenario
- `[BeforeScenario("@Integration")]` - Setup for integration tests
- `[BeforeScenario("@Performance")]` - Start performance timer

**Example:**
```csharp
[BeforeScenario]
public void BeforeScenario()
{
    _scenarioContext["Initialized"] = true;
    Console.WriteLine($"[Scenario Started] {_scenarioContext.ScenarioInfo.Title}");
}

[AfterScenario]
public void AfterScenario()
{
    if (_scenarioContext.ContainsKey("HttpClient"))
    {
        var httpClient = _scenarioContext["HttpClient"] as HttpClient;
        httpClient?.Dispose();
    }
}
```

## ScenarioContext Usage

Steps communicate via `ScenarioContext`, a dictionary that persists across all steps in a scenario:

```csharp
// Store data
_context["CurrentCurve"] = curvePoints;

// Retrieve data
var curve = (List<YieldPoint>)_context["CurrentCurve"];

// Check existence
if (_context.ContainsKey("BuildException"))
{
    var exception = _context["BuildException"] as Exception;
}
```

## Running the Tests

### Prerequisites

Ensure SpecFlow is installed:
```bash
dotnet add TermStructure.Tests package SpecFlow
dotnet add TermStructure.Tests package SpecFlow.xUnit
dotnet add TermStructure.Tests package FluentAssertions
```

### Run All BDD Tests

```bash
dotnet test TermStructure.Tests --configuration Release
```

### Run Specific Feature File

```bash
dotnet test TermStructure.Tests --filter "Feature=Building interest rate curves"
```

### Run Scenarios with Tag

```bash
dotnet test TermStructure.Tests --filter "Category=Integration"
```

### Run with Verbose Output

```bash
dotnet test TermStructure.Tests --logger "console;verbosity=detailed"
```

### Generate SpecFlow Report

```bash
specflow list .
```

## Test Coverage Map

### Feature: CurveBuilding.feature

| Scenario | Covers | Strategy | Status |
|----------|--------|----------|--------|
| Bootstrap with valid bonds | BootstrapCurveBuildingStrategy.BuildCurve() | Bootstrap | ✅ |
| Linear strategy | LinearCurveBuildingStrategy.BuildCurve() | Linear | ✅ |
| Nelson-Siegel | NelsonSiegelCurveBuildingStrategy.BuildCurve() | Nelson-Siegel | ✅ |
| Interpolation at maturity | LinearInterpolationStrategy.Interpolate() | Interpolation | ✅ |
| Densify curve | LinearInterpolationStrategy.Densify() | Densification | ✅ |
| Tenor combinations | RateCurveBuilder.Build() | All | ✅ |
| Unsorted input | BootstrapCurveBuildingStrategy sorting logic | Bootstrap | ✅ |

### Feature: DataIntegration.feature

| Scenario | Covers | Method | Status |
|----------|--------|--------|--------|
| Fetch latest FRED value | FredDataFetcher.GetLatestValueAsync() | FRED API | ✅ |
| Bulk observations | FredDataFetcher.GetSeriesObservationsAsync() | FRED API | ✅ |
| Build from FRED | RateCurveBuilder + FRED | Integration | ✅ |
| Data conversion | InterestRate model mapping | Mapping | ✅ |
| Data quality validation | decimal.TryParse() | Parsing | ✅ |
| Caching | Cache logic | Caching | ✅ |

### Feature: BusinessWorkflows.feature

| Scenario | Covers | Use Case | Status |
|----------|--------|----------|--------|
| Complete bootstrap workflow | End-to-end | Portfolio management | ✅ |
| Curve shape analysis | Yield curve characteristics | Analysis | ✅ |
| Curve inversion detection | Curve slope analysis | Economic signal | ✅ |
| Bond pricing | DCF with curve | Pricing | ✅ |
| Comparative analysis | Multi-instrument | Relative value | ✅ |
| Densification for reporting | Curve interpolation | Reporting | ✅ |
| Strategy comparison | Bootstrap vs Linear | Methodology | ✅ |
| Curve updates | Data refresh | Real-time | ✅ |
| Scenario analysis | Parallel shifts | Risk analysis | ✅ |
| Pattern detection | Multi-period analysis | Forecasting | ✅ |

### Feature: EdgeCasesAndErrors.feature

| Scenario | Covers | Error Type | Status |
|----------|--------|-----------|--------|
| Empty bonds | Empty collection handling | Input validation | ✅ |
| Empty rates | Empty collection handling | Input validation | ✅ |
| Invalid bond data | Negative/invalid values | Data validation | ✅ |
| Missing tenor | Null/empty strings | Required fields | ✅ |
| Malformed tenor | Format parsing | Format validation | ✅ |
| Edge case tenors | Boundary values | Parsing edge cases | ✅ |
| Decimal precision | Rounding artifacts | Precision | ✅ |
| Insufficient Nelson-Siegel data | < 3 observations | Algorithm requirements | ✅ |
| Single point interpolation | Boundary condition | Extrapolation prevention | ✅ |
| Interpolation boundaries | Beyond curve limits | Boundary handling | ✅ |
| HTTP errors | Network failures | API errors | ✅ |
| Malformed JSON | JSON parsing errors | Deserialization | ✅ |
| Missing API key | Initialization | Configuration errors | ✅ |
| Unparseable rates | Invalid decimals | Parsing errors | ✅ |
| Duplicate maturities | Same maturity bonds | Consolidation | ✅ |
| Small maturities | Fractional years | Numerical handling | ✅ |
| Large maturities | 30-50 years | Numerical stability | ✅ |
| Concurrent requests | Thread safety | Concurrency | ✅ |

## Expected Coverage

**Target Coverage:** 90% minimum across all modules

### By Module

| Module | Features Tested | Min Coverage | Target |
|--------|-----------------|--------------|--------|
| Builders (RateCurveBuilder) | All scenarios | 90% | 98% |
| Strategies (Building) | All scenarios | 90% | 98% |
| Strategies (Interpolation) | Interpolation/Densify | 90% | 98% |
| Services (FredDataFetcher) | Data integration | 85% | 90% |
| Models | Data conversion | 80% | 85% |

## Assertion Patterns

### Common Assertions

```csharp
// Point count
Assert.Equal(expectedCount, curve.Count);

// Rate ranges
Assert.True(rate >= min && rate <= max);

// Ordering
var sorted = curve.OrderBy(p => p.Maturity).ToList();
Assert.Equal(sorted, curve);

// No exceptions
Assert.Null(exception);

// Approximate equality
Assert.True(Math.Abs(actual - expected) < 0.0001m);

// Null checks
Assert.NotNull(curve);
Assert.Empty(curve);
```

## Scenario Outline Examples

Scenario Outlines with Examples reduce duplication:

```gherkin
Scenario Outline: Build curves with different tenor combinations
  Given I have interest rate observations for the following tenors:
    | Tenor |
    | <tenor1> |
    | <tenor2> |
    | <tenor3> |
  When I build the yield curve
  Then the curve should contain <expected_points> yield points

  Examples:
    | tenor1 | tenor2 | tenor3 | expected_points |
    | 1Y     | 5Y     | 10Y    | 3               |
    | 6M     | 1Y     | 2Y     | 3               |
    | 2Y     | 7Y     | 30Y    | 3               |
```

## Mock Setup Pattern

FRED API tests use `FakeHttpHandler` to simulate responses:

```csharp
[Given(@"the FRED service has data for series ""(.*)""")]
public void GivenFredHasData(string seriesId)
{
    var response = CreateFredSeriesResponse(seriesId, new[] { 3.25m });
    var httpResponse = new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(response, Encoding.UTF8, "application/json")
    };

    _fakeHandler = new FakeHttpHandler(httpResponse);
    _httpClient = new HttpClient(_fakeHandler);
    _fredFetcher = new FredDataFetcher(_httpClient, "test-api-key");
}
```

## Best Practices

### 1. **One Behavior Per Scenario**
Each scenario tests a single business behavior, not multiple paths.

### 2. **Clear Gherkin Language**
Use business terminology, not implementation details:
- ✅ "Build a yield curve from bond quotes"
- ❌ "Call BootstrapCurveBuildingStrategy.BuildCurve()"

### 3. **Given-When-Then Structure**
- **Given:** Preconditions and test data
- **When:** The action being tested
- **Then:** Expected outcomes

### 4. **Reusable Step Definitions**
Steps use regex patterns to match multiple similar scenarios:
```csharp
[Given(@"I have interest rate observations for the following tenors:")]
public void GivenTenorObservations(DataTable dataTable) { ... }
```

### 5. **ScenarioContext for Communication**
All steps in a scenario share `ScenarioContext`, not method parameters.

## Continuous Integration

Add to CI/CD pipeline:

```yaml
# .github/workflows/bdd-tests.yml
- name: Run BDD Tests
  run: dotnet test TermStructure.Tests --configuration Release
  
- name: Verify Coverage
  run: dotnet test /p:CollectCoverage=true /p:CoverageThreshold=90
```

## Future Enhancements

- [ ] Performance benchmarks for curve building (target < 10ms for bootstrap)
- [ ] Stress testing with 1000+ data points
- [ ] Actual FRED API integration tests (currently mocked)
- [ ] Curve analytics (duration, convexity, key rates)
- [ ] Portfolio risk analysis workflows
- [ ] Multi-currency curve scenarios
- [ ] Curve smoothing and spline interpolation strategies

## References

- SpecFlow Documentation: https://specflow.org/
- Gherkin Reference: https://cucumber.io/docs/gherkin/
- xUnit.net: https://xunit.net/
- TermStructure Project Architecture: See ARCHITECTURE.md
