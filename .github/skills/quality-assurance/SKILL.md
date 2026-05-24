---
name: quality-assurance
description: "Run comprehensive QA checks: unit tests, code coverage (90% threshold), performance benchmarks, and documentation validation. Use when preparing a PR, before merging, or during release cycles to ensure code quality."
argument-hint: "Optional: specific check to run (tests|coverage|performance|docs|all). Default: all"
user-invocable: true
---

# Quality Assurance Workflow

Automate comprehensive quality checks on your C# codebase to maintain code quality, test coverage, performance, and documentation standards.

## When to Use

- **Before creating a pull request** — Ensure all checks pass
- **During code review** — Verify quality metrics
- **Before release/merge to main** — Full QA gate
- **After significant changes** — Validate no regressions
- **Weekly quality audit** — Track trends in coverage and performance

## What This Skill Does

1. ✅ **Runs all unit tests** (xUnit) — Validates functionality
2. 📊 **Measures code coverage** (Coverlet) — Ensures 90%+ coverage
3. 🚀 **Runs performance benchmarks** (BenchmarkDotNet) — Detects regressions
4. 📚 **Validates documentation** (Docfx) — Ensures API docs are current

## Workflow Diagram

```
Quality Assurance Checks
├─ Unit Tests (parallel)
│  └─ TermStructure.Tests
├─ Code Coverage (parallel)
│  ├─ Coverlet coverage report
│  └─ Validate 90% threshold
├─ Performance Benchmarks
│  └─ BenchmarkDotNet results
└─ Documentation Checks
   ├─ API documentation exists
   └─ Docstring coverage validation
```

## Quick Start

### Run All Checks
```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

### Run Specific Check
```bash
# Tests only
dotnet test TermStructure.Tests

# Coverage only
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Benchmarks
dotnet run --project TermStructure.Benchmarks --configuration Release

# Documentation
docfx docfx.json
```

---

## Detailed Procedures

### Step 1: Unit Tests

**What it checks:** All test cases pass, no broken functionality.

**Run:**
```bash
cd /Users/thebeast/Code/IRCurves
dotnet test --configuration Release --logger="console;verbosity=detailed"
```

**Success criteria:**
- ✅ All tests pass
- ❌ No failed test cases
- ⏱️ Execution time reasonable (<5 minutes)

**See:** [Test Guide](./references/testing.md)

---

### Step 2: Code Coverage (Parallel with Tests)

**What it checks:** Minimum 90% code coverage across the codebase.

**Run:**
```bash
dotnet test --configuration Release \
  /p:CollectCoverage=true \
  /p:CoverageFormat=opencover \
  /p:Exclude="[*Tests]*" \
  /p:CoverageThreshold=90
```

**Success criteria:**
- ✅ Overall coverage ≥ 90%
- ✅ No classes below 80%
- ✅ Core logic (Builders, Strategies) > 95%

**Review coverage report:**
```bash
open coverage.opencover.xml  # View in VS Code Coverage Gutters extension
```

**See:** [Coverage Guide](./references/coverage.md)

---

### Step 3: Performance Benchmarks

**What it checks:** No performance regressions in critical paths.

**Setup (one-time):**
```bash
cd TermStructure.Benchmarks
dotnet build --configuration Release
```

**Run:**
```bash
dotnet run --project TermStructure.Benchmarks --configuration Release
```

**Success criteria:**
- ✅ No significant slowdowns (>10% regression)
- ✅ Curve building operations complete in <100ms
- ✅ Interpolation calls <1ms

**Review results:** `BenchmarkDotNet.Artifacts/results/`

**See:** [Performance Guide](./references/performance.md)

---

### Step 4: Documentation Checks

**What it checks:** API documentation is complete and up-to-date.

**Run:**
```bash
docfx docfx.json
```

**Success criteria:**
- ✅ All public methods have XML doc comments
- ✅ No "TODO" or "FIXME" in comments
- ✅ Classes and interfaces documented with examples
- ✅ Docfx build succeeds with no warnings

**Review docs:**
```bash
open _site/index.html
```

**See:** [Documentation Guide](./references/documentation.md)

---

## Automated Execution (CI/CD)

To run QA checks automatically on every push/PR, create `.github/workflows/quality-assurance.yml`:

```yaml
name: Quality Assurance
on: [push, pull_request]
jobs:
  qa:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.x
      - run: dotnet test --configuration Release /p:CollectCoverage=true /p:CoverageFormat=opencover
      - run: dotnet run --project TermStructure.Benchmarks --configuration Release || true
      - run: docfx docfx.json || true
```

---

## Configuration Files

See `./assets/` for:
- `coverlet.runsettings` — Coverage settings
- `docfx-template.json` — Documentation template
- `benchmark-config.json` — Performance benchmark settings

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Coverage report not generated | Install Coverlet: `dotnet add package coverlet.collector` |
| Benchmarks won't run | Ensure Release configuration: `--configuration Release` |
| Docfx not found | Install: `dotnet tool install -g docfx` |
| Tests timeout | Run in parallel: `dotnet test --maxcpucount` |

---

## Integration with VS Code

- **Test Explorer**: VS Code C# extension shows test results inline
- **Coverage Gutters**: [Install extension](https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters) to visualize coverage
- **Code Metrics**: Watch folder to track over time

---

## Metrics Dashboard

Track these metrics over time:

```
Week 1: Coverage 87% → Week 2: 91% ✅
Week 1: Avg test time 3.2s → Week 2: 2.8s ✅
Week 1: 2 failing tests → Week 2: 0 ✅
```

Maintain a [QUALITY.md](./assets/QUALITY-TEMPLATE.md) in your repo root to document targets.

---

## Next Steps

After all checks pass:
1. ✅ Create/update PR with results summary
2. ✅ Tag reviewers if coverage drops
3. ✅ Update CHANGELOG if significant changes
4. ✅ Merge when approved
