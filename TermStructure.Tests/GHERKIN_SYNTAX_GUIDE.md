# Gherkin Syntax & Examples for TermStructure BDD Tests

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [Keywords](#keywords)
3. [Data Tables](#data-tables)
4. [Scenario Outlines](#scenario-outlines)
5. [Regular Expressions](#regular-expressions)
6. [Common Patterns](#common-patterns)
7. [Best Practices](#best-practices)

## Basic Syntax

### Feature Structure

```gherkin
Feature: Building interest rate curves with different strategies
  As a portfolio manager
  I want to build yield curves using different construction strategies
  So that I can price instruments and perform risk analysis
```

**Format:**
- `Feature:` - Describes the business capability (required)
- `As a...` - The actor/persona (optional but recommended)
- `I want...` - The goal (optional but recommended)
- `So that...` - The business value (optional but recommended)

### Scenario Structure

```gherkin
Scenario: Build a yield curve using bootstrap strategy with valid bonds
  Given I have bond quotes with the following details:
    | BondId  | MaturityYears | AskPrice |
    | BOND001 | 1             | 99.75    |
  When I build the yield curve from these bonds
  Then the curve should contain 3 yield points
```

**Format:**
- `Scenario:` - Unique test name (required)
- `Given` - Setup/preconditions (required)
- `When` - The action (required)
- `Then` - Expected outcome (required)

### Background

Runs before each scenario in the feature:

```gherkin
Feature: Building curves
  Background:
    Given the interpolation strategy is linear interpolation
    And the system is ready to build curves

  Scenario: Build a yield curve
    # These Given steps run first:
    # - interpolation strategy is linear
    # - system is ready
    When I build the yield curve
    Then the curve should contain 3 points
```

## Keywords

### Given (Arrange)

Sets up test data and context:

```gherkin
Given I have bond quotes with the following details:
Given the FRED API service is configured
Given I have an empty list of bonds
Given I have a yield curve with 3 points
```

### When (Act)

Performs the action being tested:

```gherkin
When I build the yield curve from these bonds
When I request the interpolated yield at 5 years
When I densify the curve with a step of 0.25 years
When I fetch the latest value from FRED
```

### Then (Assert)

Verifies expected outcomes:

```gherkin
Then the curve should contain 3 yield points
Then the curve should be sorted by maturity in ascending order
Then the interpolated rate should be approximately 0.030
Then the system should return an empty list
```

### And / But

Combine multiple steps of the same type:

```gherkin
Scenario: Curve validation
  Given I have bond quotes
  And the bonds are sorted by maturity
  And all bonds have valid prices
  When I build the curve
  Then the curve should have 3 points
  And the points should be sorted
  But there should be no negative yields
```

## Data Tables

### Basic Table Format

```gherkin
Given I have bond quotes with the following details:
  | BondId  | MaturityYears | YieldToMaturity | AskPrice |
  | BOND001 | 1             | 0.0425          | 99.75    |
  | BOND002 | 5             | 0.0480          | 98.50    |
  | BOND003 | 10            | 0.0500          | 97.00    |
```

**Usage in Step Definition:**

```csharp
[Given(@"I have bond quotes with the following details:")]
public void GivenBondQuotes(DataTable dataTable)
{
    // Convert table to list of BondQuote objects
    var bonds = dataTable.CreateSet<BondQuote>().ToList();
    
    // Table columns map to property names
    foreach (var bond in bonds)
    {
        Console.WriteLine($"{bond.BondId}: {bond.MaturityYears}Y @ {bond.AskPrice}");
    }
}
```

### Dictionary Table Format

For property-value pairs:

```gherkin
Then the resulting InterestRate should have:
  | Property | Value    |
  | Currency | USD      |
  | RateType | FRED     |
  | Rate     | 0.0325   |
```

**Usage in Step Definition:**

```csharp
[Then(@"the resulting InterestRate should have:")]
public void ThenInterestRateHasProperties(DataTable dataTable)
{
    var row = dataTable.Rows.First();
    var expected = row.Keys.ToDictionary(k => k, k => row[k]);
    
    Assert.Equal("USD", expected["Currency"]);
    Assert.Equal("FRED", expected["RateType"]);
}
```

## Scenario Outlines

### Pattern

```gherkin
Scenario Outline: Build curves with different tenors
  Given I have interest rate observations for <tenors>
  When I build the yield curve
  Then the curve should contain <expected_points> points

  Examples:
    | tenors         | expected_points |
    | 1Y, 5Y, 10Y    | 3               |
    | 6M, 1Y, 2Y     | 3               |
    | 2Y, 7Y, 30Y    | 3               |
    | 3M, 6M, 1Y     | 3               |
```

**How it works:**
- Replace `<placeholder>` with values from Examples table
- Each row = one scenario execution
- Generates 4 separate test cases

**Step definition (one implementation handles all rows):**

```csharp
[Given(@"I have interest rate observations for (.*)")]
public void GivenObservations(string tenorList)
{
    var tenors = tenorList.Split(", ");
    // tenors = ["1Y", "5Y", "10Y"] for first example
}

[When(@"I build the yield curve")]
public void WhenBuildCurve()
{
    // Runs for each example row
}

[Then(@"the curve should contain (\d+) points")]
public void ThenPointCount(int expectedPoints)
{
    // Runs for each example row
    Assert.Equal(expectedPoints, curve.Count);
}
```

## Regular Expressions

### Matching Numbers

```gherkin
When I request the interpolated yield at 5 years
When I densify with step of 0.25 years
When I request value for 10Y treasury
```

**Regex patterns:**

```csharp
[When(@"I request the interpolated yield at (\d+) years")]
// Matches: "at 5 years", "at 10 years"
// Captures: 5, 10

[When(@"I densify with step of ([\d.]+) years")]
// Matches: "0.25 years", "1.5 years"
// Captures: 0.25, 1.5

[When(@"I request value for (\d+)Y treasury")]
// Matches: "10Y treasury", "5Y treasury"
// Captures: 10, 5
```

### Common Patterns

```csharp
// Decimal numbers
(\d+\.?\d*)         // 0.25, 1, 3.14159
([0-9]+\.?[0-9]*)   // Same as above

// Text/Strings
(.*)                // Any text (greedy)
([a-zA-Z]+)         // Letters only
([A-Z0-9]+)         // Uppercase letters or digits

// Named values
(bootstrap|linear|nelson)   // One of these
(Given|When|Then)           // One of these

// Date/Time formats
(\d{4}-\d{2}-\d{2})         // 2026-05-27
(\d+:\d{2})                 // 14:30

// Currency/Amounts
\$(\d+\.?\d*)               // $100.50
(\d+)% or (\d+) percent     // 3.5% or 3.5 percent
```

### Step Definition Examples

```csharp
// Simple number matching
[Given(@"I have (\d+) bonds")]
public void GivenBondCount(int count) { }

// Multiple captures
[When(@"I build curves for (.*) and (.*)")]
public void WhenBuildCurves(string strategy1, string strategy2) { }

// Optional text
[Then(@"the curve should( not)? be sorted")]
public void ThenSorted(string negation) 
{ 
    bool shouldBeSorted = string.IsNullOrEmpty(negation);
}

// Decimal/percentage
[Then(@"the rate should be approximately (.*)")]
public void ThenRateIs(string rateStr)
{
    decimal rate = decimal.Parse(rateStr);
}

// Tenor parsing
[Given(@"I have bonds with tenors ([\d]+[YMD] separated by commas)")]
public void GivenBondsWithTenors(string tenorList) { }
```

## Common Patterns

### Pattern 1: Setup with Tables

```gherkin
Given I have bond quotes with the following details:
  | BondId | MaturityYears | AskPrice |
  | B001   | 1             | 99.75    |
  | B002   | 5             | 98.50    |
```

```csharp
[Given(@"I have bond quotes with the following details:")]
public void GivenBondQuotes(DataTable dataTable)
{
    _bonds = dataTable.CreateSet<BondQuote>().ToList();
    _context["Bonds"] = _bonds;
}
```

### Pattern 2: Action with Parameters

```gherkin
When I request the interpolated yield at 3.5 years
When I build the curve using bootstrap strategy
When I fetch data for series DGS10
```

```csharp
[When(@"I request the interpolated yield at ([\d.]+) years")]
public void WhenInterpolate(string maturityStr)
{
    decimal maturity = decimal.Parse(maturityStr);
    var rate = interpolationStrategy.Interpolate(maturity, curve);
    _context["InterpolatedRate"] = rate;
}
```

### Pattern 3: Assertion with Context

```gherkin
Then the curve should contain 3 yield points
Then the interpolated rate should be approximately 0.030
```

```csharp
[Then(@"the curve should contain (\d+) yield points")]
public void ThenCurveContains(int expectedCount)
{
    var curve = (List<YieldPoint>)_context["CurrentCurve"];
    Assert.Equal(expectedCount, curve.Count);
}

[Then(@"the interpolated rate should be approximately (.*)")]
public void ThenRateApprox(string expectedStr)
{
    decimal expected = decimal.Parse(expectedStr);
    var actual = (decimal)_context["InterpolatedRate"];
    Assert.True(Math.Abs(actual - expected) < 0.0001m);
}
```

### Pattern 4: Error Handling

```gherkin
When I attempt to build a curve
Then the system should raise a FormatException
And the error message should indicate the invalid tenor
```

```csharp
[When(@"I attempt to build a curve")]
public void WhenAttemptBuild()
{
    try
    {
        _curve = builder.Build(_bonds, _rates);
        _context["Exception"] = null;
    }
    catch (Exception ex)
    {
        _context["Exception"] = ex;
    }
}

[Then(@"the system should raise a FormatException")]
public void ThenFormatException()
{
    var exception = _context["Exception"] as Exception;
    Assert.IsType<FormatException>(exception);
}
```

### Pattern 5: Multiple Assertions

```gherkin
Then the curve should contain 3 yield points
And the curve should be sorted by maturity in ascending order
And all yields should be positive
```

```csharp
[Then(@"the curve should contain (\d+) yield points")]
public void ThenPointCount(int count) { ... }

[Then(@"the curve should be sorted by maturity in ascending order")]
public void ThenSorted() { ... }

[Then(@"all yields should be positive")]
public void ThenPositiveYields() { ... }
```

## Best Practices

### ✅ DO

**Use Business Language**
```gherkin
# ✅ Good
Scenario: Build a yield curve using bootstrap strategy
  When I build the yield curve from these bonds
  Then the curve should contain 3 points

# ❌ Bad
Scenario: Call BootstrapCurveBuildingStrategy.BuildCurve()
  When I call builder.Build()
  Then result.Count equals 3
```

**One Behavior Per Scenario**
```gherkin
# ✅ Good
Scenario: Build curve returns sorted points
  When I build the curve
  Then the curve should be sorted by maturity

# ❌ Bad
Scenario: Build curve and verify all properties
  When I build the curve
  Then the curve should have 3 points
  And points should be sorted
  And all yields should be positive
  And rates should be within bounds
```

**Use Descriptive Names**
```gherkin
# ✅ Good
Scenario: Handle empty bond collection gracefully
Scenario: Detect inverted yield curve
Scenario: Bootstrap curve with 3-year bonds

# ❌ Bad
Scenario: Test 1
Scenario: Error scenario
Scenario: Build curve
```

**Use Tables for Complex Data**
```gherkin
# ✅ Good
Given I have bond quotes:
  | BondId | Maturity | Price |
  | B001   | 1        | 99.75 |

# ❌ Bad
Given I have bond B001 with maturity 1 and price 99.75
```

### ❌ DON'T

**Don't Use Implementation Details**
```gherkin
# ❌ Bad
When I call BootstrapCurveBuildingStrategy.BuildCurve()
And I pass IEnumerable<BondQuote> parameter
Then _discountFactors should be populated

# ✅ Good
When I build the yield curve using bootstrap
Then the curve should contain yield points
```

**Don't Mix Multiple Behaviors**
```gherkin
# ❌ Bad - too many behaviors
Scenario: Complete curve workflow
  Given I have bond data
  When I build the curve
  And I interpolate at 3 years
  And I densify the curve
  And I compare with another curve
  Then ...

# ✅ Good - focused
Scenario: Build curve with bond data
  Given I have bond data
  When I build the curve
  Then the curve should contain points
```

**Don't Hardcode Too Much**
```gherkin
# ❌ Bad - too specific
Scenario: Build specific bond curve
  Given I have exactly 3 bonds
  With maturities 1, 5, 10
  And prices 99.75, 98.50, 97.00

# ✅ Good - flexible with examples
Scenario Outline: Build curve with different bonds
  Given I have <bond_count> bonds
  When I build the curve
  Then the curve should contain <point_count> points
  
  Examples:
    | bond_count | point_count |
    | 3          | 3           |
    | 5          | 5           |
```

## Advanced Patterns

### Scenario with Complex Context

```gherkin
Scenario: Build and analyze yield curve
  Given I have bootstrapped a yield curve
  When I request interpolated yields at 2Y, 4Y, and 7Y
  Then the interpolated rates should form a smooth curve
  And short-term rates should be lower than long-term rates
```

### Using Tags for Organization

```gherkin
@Bootstrap @Performance
Scenario: Bootstrap large portfolio

@Integration @Slow
Scenario: Fetch data from FRED API

@Unit @Fast
Scenario: Interpolate at intermediate maturity
```

**Run specific tags:**
```bash
dotnet test --filter "Category=Bootstrap"
dotnet test --filter "Category=Performance"
```

### Scenario Outline with Multiple Examples

```gherkin
Scenario Outline: Build curves with different strategies
  Given I have <bond_count> bonds
  When I use the <strategy> strategy
  Then the curve should have <point_count> points
  
  Examples: Bootstrap Strategy
    | bond_count | strategy | point_count |
    | 3          | bootstrap | 3          |
    | 5          | bootstrap | 5          |
  
  Examples: Linear Strategy
    | bond_count | strategy | point_count |
    | 2          | linear   | 2           |
    | 4          | linear   | 4           |
```

## Summary

| Keyword | Purpose | Example |
|---------|---------|---------|
| Feature | Business capability | Building yield curves |
| Scenario | Single test case | Bootstrap with 3 bonds |
| Given | Preconditions | I have bond quotes |
| When | Action | I build the curve |
| Then | Expected outcome | The curve contains 3 points |
| And | Combine steps | And all yields are positive |
| Background | Pre-scenario setup | Common Given steps |
| Scenario Outline | Data-driven | Multiple Examples rows |
| Examples | Data rows | Table with test values |

---

**Remember:** Good Gherkin reads like documentation that both business and technical teams understand!
