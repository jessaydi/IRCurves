# Project Documentation for TermStructure

This skill auto-generates and maintains README.md for the TermStructure C# project.

## How It Works

### 1. Code Analysis
```
Scan project structure
  ↓
Identify namespaces, classes, interfaces
  ↓
Extract public APIs and XML docs
  ↓
Count tests and measure coverage
  ↓
Detect design patterns
```

### 2. Documentation Generation
```
Analyze findings
  ↓
Map to README sections
  ↓
Generate markdown with code examples
  ↓
Preserve custom user content
  ↓
Update timestamps and metadata
```

### 3. Continuous Maintenance
```
On every commit/PR:
  - Check for outdated docs
  - Update outdated sections
  - Preserve custom content
  - Track changes
  - Commit updated README
```

---

## TermStructure-Specific Content

### Overview Section
The project builds interest rate term structures using curve building strategies. It's used for financial calculations involving bond yields and interest rate interpolation.

### Architecture Insights
- **Strategy Pattern**: Pluggable curve building (`BootstrapCurveBuildingStrategy`, `LinearCurveBuildingStrategy`, `NelsonSiegelCurveBuildingStrategy`)
- **Builder Pattern**: `RateCurveBuilder` and `YieldCurveBuilder` with template method
- **Interface Segregation**: `ICurveBuildingStrategy`, `IInterpolationStrategy` (SOLID principle)
- **Dependency Injection**: Constructor-based DI for testing and flexibility

### Key Classes to Document
- `RateCurveBuilder` — Main curve construction
- `FredDataFetcher` — External API integration
- `YieldPoint`, `BondQuote` — Core data models
- `ICurveBuildingStrategy` — Pluggable algorithms

### Test Organization
- `FredDataTests` — API data fetching (5 tests)
- `RateCurveBuilderTests` — Curve building (26 tests)
- Coverage targets: 90% overall, 95% for Builders/Strategies

---

## Customization Points

### Add Custom Sections
```markdown
<!-- CUSTOM: Keep this -->
## My Custom Notes
This section won't be overwritten by auto-updates.
<!-- END CUSTOM -->
```

### Update Only Specific Sections
```bash
/project-documentation update architecture api testing
```

### Skip Auto-Updates for Files
Add to `.github/docs-config.json`:
```json
{
  "skip_auto_update": ["README.md"],
  "preserve_sections": ["Contributing", "License"]
}
```

---

## Command Reference

### Generate New README
```bash
/project-documentation new
```
Creates `README.md` from scratch with all 7 sections.

### Update All Sections
```bash
/project-documentation update
```
Refreshes entire README with latest code analysis.

### Update Specific Sections
```bash
/project-documentation update architecture api testing
```
Only updates the named sections, preserves others.

### Show Update History
```bash
/project-documentation history
```
Lists all documentation changes with timestamps.

### Dry Run (Preview Changes)
```bash
/project-documentation update --dry-run
```
Shows what would change without actually modifying files.

---

## Integration Examples

### With GitHub Actions
```yaml
# .github/workflows/docs.yml
on: [push]
jobs:
  update-docs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: /project-documentation update
      - run: git add README.md && git commit -m "docs: auto-update" || true
      - run: git push
```

### With PR Comments
When a PR adds tests:
- Skill detects new tests automatically
- Updates Testing section
- Comments on PR with coverage change
- Suggests updating API section if new classes added

### With Release Workflow
```bash
# Before release
/project-documentation update
# This ensures README matches the code being released
git tag v1.0.0
```
