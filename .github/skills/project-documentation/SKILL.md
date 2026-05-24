---
name: project-documentation
description: "Create or update README.md with comprehensive project documentation. Use when starting a new project, after adding features, or before releasing. Scans codebase to auto-populate architecture, APIs, and setup instructions."
argument-hint: "Optional: 'new' for fresh README, 'update' to refresh existing, or 'sections' to update only specific parts (e.g., 'architecture,api')"
user-invocable: true
---

# Project Documentation Skill

Automatically generate and maintain a comprehensive README.md that documents your TermStructure project. Scans code, detects features, and keeps documentation in sync with implementation.

## When to Use

- **Project kickoff** — Generate initial README with project structure
- **After adding features** — Update relevant sections automatically
- **Before releases** — Ensure documentation reflects current state
- **Quarterly audits** — Refresh all sections, check for outdated info
- **CI/CD integration** — Auto-update on every push/PR merge
- **Onboarding new team members** — Keep docs current and comprehensive

## What This Skill Does

1. **Scans codebase** — Discovers project structure, classes, APIs, test coverage
2. **Extracts metadata** — Pulls version, dependencies, frameworks from config files
3. **Generates README** — Creates comprehensive documentation with 7 key sections
4. **Updates intelligently** — Only modifies outdated sections, preserves custom notes
5. **Tracks changes** — Records when documentation was last updated

## Documentation Sections

| Section | Purpose | Auto-Generated From |
|---------|---------|-------------------|
| **Project Overview** | What the project does and why | Project structure + `.csproj` metadata |
| **Architecture** | System design, components, patterns | Folder structure + interface definitions |
| **Installation** | Setup and build instructions | `.csproj` files + dependencies |
| **Usage Examples** | How to use the main APIs | Public method signatures + XML docs |
| **API Documentation** | Complete API reference | Class/method definitions + comments |
| **Testing** | Test structure and coverage goals | Test files + coverage thresholds |
| **Contributing** | Contribution guidelines | `.github/` customizations |

## Quick Start

### Generate New README
```bash
# Creates README.md if it doesn't exist
/project-documentation new
```

### Update Entire Documentation
```bash
# Refreshes all sections with latest code analysis
/project-documentation update
```

### Update Specific Sections
```bash
# Only update architecture and API sections
/project-documentation update architecture,api
```

### Track Changes Over Time
```bash
# Shows what changed in documentation
/project-documentation history
```

---

## Detailed Procedure

### Step 1: Analyze Project Structure

**Action**: Scan the codebase to understand the project

**Automated Discovery**:
- ✅ Read project files: `TermStructure.csproj`, `TermStructure.Tests.csproj`
- ✅ Identify namespace structure: `TermStructure.Builders`, `TermStructure.Services`, etc.
- ✅ Count lines of code per module
- ✅ Extract framework version and key dependencies

**Example Output**:
```
Project: TermStructure (C# .NET 9.0)
├─ Builders/          (3 classes, ~200 LOC)
├─ Services/          (2 classes, ~300 LOC)
├─ Strategies/        (5 interfaces + 4 implementations, ~400 LOC)
├─ Models/            (5 data classes, ~100 LOC)
└─ Tests/             (31 tests, 90%+ coverage target)
```

---

### Step 2: Extract Public APIs

**Action**: Discover all public classes, interfaces, and key methods

**Automated Discovery**:
- ✅ Parse `*.cs` files for `public class`, `public interface`
- ✅ Extract method signatures with parameters
- ✅ Read XML documentation (`/// <summary>`)
- ✅ Identify patterns: Abstract classes, interfaces, strategies

**Example Extraction**:
```csharp
public abstract class RateCurveBuilder
{
    public List<YieldPoint> Build(IEnumerable<BondQuote> bonds, ...)
    public decimal GetRate(decimal maturity, ...)
    public List<YieldPoint> Densify(decimal step, ...)
}
```

**Generates API Section**:
```markdown
### RateCurveBuilder

Builds interest rate term structures using curve building strategies.

**Methods**:
- `Build(bonds, rates)` → List<YieldPoint>
- `GetRate(maturity, bonds, rates)` → decimal
- `Densify(step, bonds, rates)` → List<YieldPoint>
```

---

### Step 3: Scan Test Suite

**Action**: Analyze test coverage and structure

**Automated Discovery**:
- ✅ Count total tests and pass rate
- ✅ Extract test class names: `FredDataTests`, `RateCurveBuilderTests`
- ✅ Identify coverage thresholds from CI config
- ✅ List critical test categories (happy path, edge cases, integration)

**Generates Testing Section**:
```markdown
### Testing

**Test Framework**: xUnit + Moq
**Coverage Target**: 90% minimum

**Test Suite**:
- FredDataTests (5 tests) — API data fetching
- RateCurveBuilderTests (26 tests) — Curve building logic
  - Happy paths, edge cases, integration tests
  - Coverage: Builders (95%), Strategies (95%), Services (85%)

**Running Tests**:
\`\`\`bash
dotnet test TermStructure.Tests --configuration Release
\`\`\`
```

---

### Step 4: Document Architecture

**Action**: Explain design patterns and module responsibilities

**Analysis Identifies**:
- ✅ Strategy pattern in `Strategies/`
- ✅ Builder pattern in `Builders/`
- ✅ Dependency injection via constructors
- ✅ SOLID principles alignment

**Generates Architecture Section**:
```markdown
### Architecture

**Design Patterns**:
- **Strategy Pattern**: Pluggable curve building and interpolation algorithms
- **Builder Pattern**: Fluent curve construction
- **Dependency Injection**: Constructor-based DI with mocking support
- **SOLID Principles**: Single Responsibility, Interface Segregation, Dependency Inversion

**Module Breakdown**:
- `Builders/`: RateCurveBuilder, YieldCurveBuilder (template method pattern)
- `Strategies/`: ICurveBuildingStrategy, IInterpolationStrategy implementations
- `Services/`: FredDataFetcher (external API integration)
- `Models/`: YieldPoint, BondQuote, InterestRate (data transfer objects)

**Dependency Flow**:
FredDataFetcher → RateCurveBuilder → ICurveBuildingStrategy + IInterpolationStrategy
```

---

### Step 5: Extract Installation & Setup

**Action**: Generate setup instructions from project files

**Automated Discovery**:
- ✅ Parse `.csproj` for target framework (`net9.0`)
- ✅ List dependencies from package references
- ✅ Extract build commands from `package.json` or MSBuild targets

**Generates Installation Section**:
```markdown
### Installation

**Prerequisites**:
- .NET 9.0 SDK or later
- Git

**Clone and Build**:
\`\`\`bash
git clone https://github.com/yourorg/IRCurves.git
cd IRCurves
dotnet build --configuration Release
\`\`\`

**Run Tests**:
\`\`\`bash
dotnet test TermStructure.Tests --configuration Release
\`\`\`

**Dependencies**:
- Moq 4.20.72 (unit testing)
- Coverlet (code coverage)
```

---

### Step 6: Generate Usage Examples

**Action**: Create code examples from public APIs and XML docs

**Source Material**:
- ✅ Public method signatures and documentation
- ✅ Test fixtures and test data
- ✅ Existing example code in comments

**Generates Usage Section**:
```markdown
### Usage Examples

#### Building a Yield Curve

\`\`\`csharp
var bonds = new List<BondQuote> { /* ... */ };
var rates = new List<InterestRate> { /* ... */ };

var builder = new YieldCurveBuilder(
    new LinearCurveBuildingStrategy(),
    new LinearInterpolationStrategy()
);

var curve = builder.Build(bonds, rates);
\`\`\`

#### Getting an Interpolated Rate

\`\`\`csharp
var rate = builder.GetRate(5.5m, bonds, rates); // Get rate at 5.5 year maturity
\`\`\`
```

---

### Step 7: Update Automatically

**For CI/CD Integration** (GitHub Actions):

Add to `.github/workflows/documentation.yml`:

```yaml
name: Update Documentation
on: [push]
jobs:
  docs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: /project-documentation update
      - run: |
          git config user.name "Docs Bot"
          git config user.email "docs@example.com"
          git add README.md
          git commit -m "docs: auto-update README" || true
          git push
```

This ensures README.md stays in sync with code changes automatically.

---

## README Structure

The generated README.md follows this structure:

```markdown
# TermStructure

[1-line project description]

## Overview

[Multi-paragraph explanation of what the project does and why]

## Architecture

[Design patterns, module breakdown, dependency flow]

## Installation & Setup

[Prerequisites, build steps, configuration]

## Usage Examples

[Code examples for main use cases]

## API Reference

[Complete API documentation]

## Testing & Quality

[Test framework, coverage, how to run tests]

## Contributing

[Guidelines for contributing to the project]

---

**Last Updated**: [Auto-generated timestamp]
**Documentation Version**: [Matches code version]
```

---

## Customization

### Preserving Custom Notes

The skill preserves any custom sections you add:

```markdown
## Custom Section
This won't be overwritten. Add your own content here.
<!-- CUSTOM_CONTENT -->
```

Sections marked with comments are preserved during updates.

### Updating Only Specific Sections

```bash
# Update only these sections
/project-documentation update overview,api

# Don't update architecture
/project-documentation update --skip architecture
```

---

## Integration with Other Skills

This skill works alongside:

- **quality-assurance**: Updates Testing section with latest coverage metrics
- **test-expert**: Includes comprehensive test documentation
- **csharp-solid-principles**: Documents architectural patterns in Architecture section

---

## Files Created

- `README.md` — Main project documentation
- `CHANGELOG.md` — Auto-tracked version history (optional)
- `.github/docs-config.json` — Documentation settings (optional)

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| README won't generate | Check that project files exist and are valid C# projects |
| API section incomplete | Verify public classes have XML documentation (`/// <summary>`) |
| Architecture section wrong | Add comments explaining design patterns |
| Tests not detected | Ensure test files end with `*Tests.cs` or `*Test.cs` |

---

## Next Steps

1. ✅ Generate initial README with `/project-documentation new`
2. ✅ Review and customize as needed
3. ✅ Enable CI/CD automation to keep docs updated
4. ✅ Share with team for feedback

---

**Related Commands**:
```
/project-documentation new              # Create fresh README
/project-documentation update           # Refresh all sections
/project-documentation update api,testing  # Update specific sections
/project-documentation history          # Show documentation changes
```
