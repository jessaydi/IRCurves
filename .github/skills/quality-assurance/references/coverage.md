# Code Coverage Guide (Coverlet)

## Overview

Code coverage measures what percentage of your code is executed by tests. Target: **90% minimum**.

## Running Coverage Analysis

### Basic Coverage
```bash
dotnet test /p:CollectCoverage=true
```

### With OpenCover Format (for visualization)
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### With Threshold Check
```bash
dotnet test /p:CollectCoverage=true /p:CoverageThreshold=90 /p:CoverageThresholdType=line
```

### Exclude Test Projects
```bash
dotnet test /p:CollectCoverage=true /p:Exclude="[*Tests]*"
```

## Interpreting Results

### Coverage Metrics

- **Line Coverage**: % of code lines executed
- **Branch Coverage**: % of code branches (if/else) executed
- **Method Coverage**: % of methods called

### What to Prioritize

1. **Core Logic** (Builders, Strategies): Aim for 95%+
2. **Services** (Data fetching): Aim for 90%+
3. **Models** (Data objects): Aim for 85%+ (less critical)
4. **Utilities**: Aim for 85%+

### What to Exclude

- Test helper classes
- Generated code (obj/, bin/)
- Program.cs entry point (can be complex)

## Coverage Report Locations

- **Summary**: Console output after `dotnet test`
- **Detailed HTML**: `coveragereport/index.html` (if using `ReportGenerator`)
- **XML**: `coverage.opencover.xml` (machine-readable)

## Integration with VS Code

### Install Coverage Gutters Extension
```bash
code --install-extension ryanluker.vscode-coverage-gutters
```

Then:
1. Run coverage: `dotnet test /p:CollectCoverage=true`
2. Watch coverage: `Cmd+Shift+P` → "Coverage Gutters: Watch"
3. View inline: Green = covered, Red = uncovered

## Improving Coverage

### Find Uncovered Code
```bash
# Generate HTML report with ReportGenerator
dotnet tool install -g ReportGenerator
ReportGenerator -reports:coverage.opencover.xml -targetdir:coveragereport -reporttype:Html
open coveragereport/index.html
```

### Common Gaps

1. **Error handling**: Add tests for exceptions
2. **Edge cases**: Test null, empty, boundary conditions
3. **Branches**: Test all if/else paths
4. **Async code**: Test success and failure paths

## Build Failure on Low Coverage

To fail the build if coverage drops:

```bash
dotnet test /p:CollectCoverage=true \
  /p:CoverageThreshold=90 \
  /p:FailOnCodeCoverage=true
```

This will exit with non-zero code if coverage < 90%.
