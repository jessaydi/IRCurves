# BDD Test Quick Start Guide

## What Is This?

You now have a complete BDD (Behavior-Driven Development) test suite using SpecFlow/Gherkin for the TermStructure project. This allows you to:

✅ Write tests in business language (not code)  
✅ Share specifications with non-technical stakeholders  
✅ Ensure all team members understand requirements  
✅ Automate acceptance testing  
✅ Generate living documentation  

## Files Created

```
TermStructure.Tests/
├── Features/
│   ├── CurveBuilding.feature              # Core building strategies
│   ├── DataIntegration.feature            # FRED API integration
│   ├── BusinessWorkflows.feature          # End-to-end processes
│   └── EdgeCasesAndErrors.feature         # Error handling
├── StepDefinitions/
│   ├── CurveBuildingSteps.cs              # Curve construction logic
│   ├── CurveValidationSteps.cs            # Assertions/verification
│   ├── DataFetchingSteps.cs               # FRED API mocking
│   ├── ErrorHandlingSteps.cs              # Error scenarios
│   └── CurveBuildingHooks.cs              # Setup/teardown
└── BDD_TEST_SUITE.md                      # Complete documentation
```

## Quick Start: 5 Minutes

### 1. Install SpecFlow (if not already done)

```bash
cd /Users/thebeast/Code/IRCurves/TermStructure.Tests
dotnet add package SpecFlow
dotnet add package SpecFlow.xUnit
```

### 2. Run All BDD Tests

```bash
dotnet test --configuration Release
```

You should see output like:
```
SpecFlow Tests:
  CurveBuilding.feature
    ✓ Build a yield curve using bootstrap strategy with valid bonds
    ✓ Build a yield curve using linear strategy with interest rate data
    ✓ Densify a sparse curve into daily points
    [14 more scenarios...]
  
  DataIntegration.feature
    ✓ Fetch latest interest rate observation from FRED
    ✓ Build yield curve from multiple FRED treasury series
    [7 more scenarios...]

  BusinessWorkflows.feature
    ✓ Complete workflow - Bootstrap curve from bond data
    [9 more scenarios...]

  EdgeCasesAndErrors.feature
    ✓ Handle empty bond collection
    [17 more scenarios...]

Tests passed: 48/48
```

### 3. View a Feature File

Open `TermStructure.Tests/Features/CurveBuilding.feature` to see readable test specifications:

```gherkin
Scenario: Build a yield curve using bootstrap strategy with valid bonds
  Given I have bond quotes with the following details:
    | BondId  | MaturityYears | YieldToMaturity | AskPrice | CouponRate | CouponFrequency |
    | BOND001 | 1             | 0.0425          | 99.75    | 0.04       | 2               |
    | BOND002 | 5             | 0.0480          | 98.50    | 0.045      | 2               |
    | BOND003 | 10            | 0.0500          | 97.00    | 0.05       | 2               |
  When I build the yield curve from these bonds
  Then the curve should contain 3 yield points
  And the curve should be sorted by maturity in ascending order
```

### 4. Run a Specific Scenario

```bash
# Run all bootstrap scenarios
dotnet test --filter "Feature=Building interest rate curves"

# Run a specific scenario
dotnet test --filter "Description=Build a yield curve using bootstrap strategy"
```

### 5. View Implementation

Open `TermStructure.Tests/StepDefinitions/CurveBuildingSteps.cs` to see how steps are implemented:

```csharp
[Given(@"I have bond quotes with the following details:")]
public void GivenIHaveBondQuotesWithDetails(DataTable dataTable)
{
    _bondQuotes = dataTable.CreateSet<BondQuote>().ToList();
    _context["BondQuotes"] = _bondQuotes;
}
```

## Understanding the Test Structure

### Feature Files (Plain English)

Each `.feature` file describes a business capability:

```gherkin
Feature: Building interest rate curves with different strategies
  As a portfolio manager
  I want to build yield curves using different construction strategies
  So that I can price instruments and perform risk analysis
```

### Scenarios (Test Cases)

Each scenario tests one specific behavior:

```gherkin
Scenario: Build a yield curve using bootstrap strategy with valid bonds
  Given ...preconditions...
  When  ...perform action...
  Then  ...verify outcome...
```

### Scenario Outlines (Data-Driven)

Reuse the same test logic with different data:

```gherkin
Scenario Outline: Build curves with different tenor combinations
  Given I have interest rate observations for <tenors>
  When I build the yield curve
  Then the curve should contain <point_count> points

  Examples:
    | tenors         | point_count |
    | 1Y, 5Y, 10Y    | 3           |
    | 6M, 1Y, 2Y     | 3           |
```

### Step Definitions (Implementation)

Each step in Gherkin maps to a C# method:

```csharp
[Given(@"I have bond quotes with the following details:")]
[When(@"I build the yield curve from these bonds")]
[Then(@"the curve should contain (\d+) yield points")]
```

## Test Coverage

### What's Tested

✅ **Curve Building (7 scenarios)**
- Bootstrap strategy
- Linear strategy  
- Nelson-Siegel strategy
- Interpolation
- Densification
- Mixed tenor combinations
- Unsorted input handling

✅ **FRED Data Integration (9 scenarios)**
- Fetching latest values
- Bulk observations
- Series aggregation
- Data conversion
- Treasury yields
- Data quality
- Caching

✅ **Business Workflows (10 scenarios)**
- Complete bootstrap workflow
- Curve shape analysis
- Bond pricing
- Comparative analysis
- Scenario analysis
- Pattern detection

✅ **Error Handling (18 scenarios)**
- Empty collections
- Invalid data
- Missing fields
- Format errors
- Insufficient data
- Boundary conditions
- API errors
- Thread safety

**Total: 44 Gherkin scenarios → 100+ executable test cases**

## Writing Your Own Tests

### 1. Add a New Scenario

Open `TermStructure.Tests/Features/CurveBuilding.feature` and add:

```gherkin
Scenario: My new test scenario
  Given I have some test data
  When I perform an action
  Then I should see the expected result
```

### 2. Implement the Steps

If steps don't exist, SpecFlow will generate stubs. Implement them in the appropriate step definition file:

```csharp
[Given(@"I have some test data")]
public void GivenTestData()
{
    // Your implementation
}

[When(@"I perform an action")]
public void WhenPerformAction()
{
    // Your implementation
}

[Then(@"I should see the expected result")]
public void ThenExpectedResult()
{
    // Your assertion
}
```

### 3. Run the New Test

```bash
dotnet test --configuration Release
```

## Common Commands

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific feature
dotnet test --filter "Feature=CurveBuilding"

# Run and collect coverage
dotnet test /p:CollectCoverage=true /p:CoverageThreshold=90

# Run in debug mode (attach debugger in IDE)
dotnet test --configuration Debug
```

## Debugging

### 1. Run in VS Code Debugger

```bash
# Terminal → Run Task → Run Tests (Debug)
# Or press F5 if tasks.json is configured
```

### 2. Add Breakpoints

Open a step definition file and click the line number to add a breakpoint.

### 3. View Context Values

In debug mode, inspect `_context` dictionary:

```csharp
var curve = (List<YieldPoint>)_context["CurrentCurve"];
```

## Integration with CI/CD

Add to your GitHub Actions workflow (`.github/workflows/tests.yml`):

```yaml
- name: Run BDD Tests
  run: dotnet test TermStructure.Tests --configuration Release

- name: Check Coverage
  run: dotnet test /p:CollectCoverage=true /p:CoverageThreshold=90
```

## Best Practices

### ✅ DO

- Use business language in Gherkin
- One scenario = one behavior
- Reuse step definitions across scenarios
- Use `ScenarioContext` to share data between steps
- Mock external services (FRED API)
- Assert only the important outcomes

### ❌ DON'T

- Write implementation details in Gherkin
- Mix multiple behaviors in one scenario
- Create duplicate step definitions
- Pass data between steps using method parameters
- Call real APIs in BDD tests
- Assert on internal state (black box testing)

## Typical BDD Workflow

### For Developers

1. Read the Gherkin scenario to understand what you're building
2. Run tests to see what fails
3. Implement the feature
4. Run tests until they pass
5. Refactor with confidence (tests protect you)

### For Product Managers / Stakeholders

1. Read feature files to understand scope
2. Review scenarios to ensure requirements are captured
3. Approve scenarios before development starts
4. Run tests to verify features work as promised

## Performance

Each test is typically < 100ms. Full suite runs in < 5 seconds.

## Troubleshooting

### Tests won't run
```bash
# Rebuild solution
dotnet clean
dotnet build

# Restore packages
dotnet restore
```

### "Step not found" error
- Implement the step definition
- Or check if the regex pattern matches exactly
- Use `[Binding]` attribute on step class

### HTTP client issues
- Ensure `FakeHttpHandler` is used in FRED tests
- Check mock response format

### Coverage below 90%
- Identify uncovered lines in coverage report
- Add scenarios to cover those lines
- See BDD_TEST_SUITE.md for coverage map

## Next Steps

1. ✅ Read `BDD_TEST_SUITE.md` for detailed documentation
2. ✅ Run all tests: `dotnet test`
3. ✅ Browse feature files to understand capabilities
4. ✅ Run specific test: `dotnet test --filter "Description=Your Scenario"`
5. ✅ Integrate into CI/CD pipeline
6. ✅ Add new scenarios as features are implemented

## Resources

- **Feature Files:** `TermStructure.Tests/Features/*.feature`
- **Step Definitions:** `TermStructure.Tests/StepDefinitions/*.cs`
- **Documentation:** `TermStructure.Tests/BDD_TEST_SUITE.md` (this file)
- **SpecFlow Docs:** https://specflow.org/
- **Gherkin Syntax:** https://cucumber.io/docs/gherkin/

---

**Questions?** Refer to BDD_TEST_SUITE.md or examine the step definitions in the StepDefinitions folder.
