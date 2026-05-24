# Documentation Best Practices

## Writing Good README Documentation

### Structure & Flow

1. **Start with the "why"** — Explain the problem the project solves
2. **Show the "what"** — Describe what the project does
3. **Demonstrate the "how"** — Provide code examples
4. **Guide the "next steps"** — Installation, contribution, resources

### Section Guidelines

#### Overview
- Keep first paragraph to 2-3 sentences max
- Answer: What is this project? Who should use it?
- Include badges for build status, coverage, version

#### Architecture
- Explain design patterns used
- Show module structure with ASCII diagrams
- Describe data flow and dependencies
- Link to detailed docs in `.github/instructions/`

#### Installation
- Assume reader is new to the project
- List all prerequisites explicitly
- Provide copy-paste commands
- Include troubleshooting for common issues

#### Usage Examples
- Start simple, progress to complex
- Include both success and error cases
- Show real data, not placeholder values
- Explain what each example demonstrates

#### API Reference
- Auto-generate from code if possible
- Link to source files for details
- Show parameter types and return values
- Include usage examples for key methods

#### Testing
- Explain test organization and naming
- Show how to run tests
- Document coverage targets
- Link to test guides and standards

#### Contributing
- Be welcoming and specific
- Link to SOLID principles and code standards
- Show the PR workflow
- List code review criteria

---

## Code Examples in Documentation

### ✅ Good Example

```csharp
// Shows realistic data, clear intent, and explains the result
var bonds = new List<BondQuote>
{
    new BondQuote { MaturityYears = 1m, YieldToMaturity = 0.0425m },
    new BondQuote { MaturityYears = 10m, YieldToMaturity = 0.0500m }
};

var builder = new YieldCurveBuilder(
    new LinearCurveBuildingStrategy(),
    new LinearInterpolationStrategy()
);

var curve = builder.Build(bonds, rates);
// Result: A smooth interpolation between the two points
```

### ❌ Poor Example

```csharp
// Vague, doesn't explain context or expected result
var builder = new YieldCurveBuilder(...);
var result = builder.Build(...);
```

---

## Keeping Documentation Updated

### Automated Generation

Use `codebase scanning` to:
- ✅ Extract APIs from public classes
- ✅ Count tests and coverage
- ✅ Detect design patterns
- ✅ Parse folder structure

### Manual Sections

Maintain these manually:
- Overview and motivation
- Usage examples and tutorials
- Contributing guidelines
- Contact and support info

### Version Markers

Mark auto-generated sections:
```markdown
<!-- AUTO-GENERATED: Do not edit manually -->
...content...
<!-- END AUTO-GENERATED -->
```

---

## README Maintenance Checklist

- [ ] Overview reflects current project purpose
- [ ] Architecture section matches actual design
- [ ] Installation instructions are current (framework version, dependencies)
- [ ] Code examples compile and run
- [ ] API documentation matches public classes
- [ ] Test coverage numbers are accurate
- [ ] Contributing guidelines are up-to-date
- [ ] Links are not broken
- [ ] No outdated framework/library references
- [ ] License and contact info current

---

## Tools for Documentation

### Markdown Linting
```bash
markdownlint README.md
```

### Link Checking
```bash
markdown-link-check README.md
```

### Code Sample Validation
```bash
# Run code examples to verify they work
dotnet script examples.csx
```

---

## Templates by Section

See the `templates/` folder for:
- `README-TEMPLATE.md` — Full README structure
- `ARCHITECTURE-TEMPLATE.md` — Design pattern documentation
- `API-TEMPLATE.md` — API reference format
- `CONTRIBUTING-TEMPLATE.md` — Contribution guidelines
