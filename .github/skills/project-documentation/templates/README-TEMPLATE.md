# {{PROJECT_NAME}}

{{ONE_LINE_DESCRIPTION}}

[![Build Status](https://github.com/{{ORG}}/{{REPO}}/workflows/CI/badge.svg)](https://github.com/{{ORG}}/{{REPO}}/actions)
[![Code Coverage](https://img.shields.io/codecov/c/github/{{ORG}}/{{REPO}})](https://codecov.io/gh/{{ORG}}/{{REPO}})
[![NuGet Version](https://img.shields.io/nuget/v/{{PACKAGE_NAME}}.svg)](https://www.nuget.org/packages/{{PACKAGE_NAME}})

## Overview

{{PROJECT_OVERVIEW}}

### Key Features

{{FEATURES_LIST}}

## Architecture

### Design Patterns

{{ARCHITECTURE_PATTERNS}}

### Module Structure

{{MODULE_BREAKDOWN}}

```
{{PROJECT_NAME}}/
├── Builders/          # Curve construction logic
├── Services/          # External API integration
├── Strategies/        # Pluggable algorithms
├── Models/            # Data transfer objects
└── Tests/             # Unit and integration tests
```

### Dependency Flow

{{DEPENDENCY_DIAGRAM}}

## Installation & Setup

### Prerequisites

- .NET {{DOTNET_VERSION}} SDK or later
- Git

### Building

```bash
git clone {{REPO_URL}}
cd {{PROJECT_NAME}}
dotnet build --configuration Release
```

### Running Tests

```bash
dotnet test {{TEST_PROJECT}} --configuration Release
```

### Dependencies

{{DEPENDENCIES_TABLE}}

## Usage Examples

### Basic Example

{{USAGE_EXAMPLE_1}}

### Advanced Example

{{USAGE_EXAMPLE_2}}

## API Reference

### Core Classes

{{API_REFERENCE}}

### Interfaces

{{INTERFACE_REFERENCE}}

## Testing & Quality

### Test Framework

- **Framework**: {{TEST_FRAMEWORK}}
- **Coverage Tool**: {{COVERAGE_TOOL}}
- **Target Coverage**: {{COVERAGE_TARGET}}

### Running Tests

```bash
# Run all tests
dotnet test TermStructure.Tests --configuration Release

# Check coverage
dotnet test /p:CollectCoverage=true /p:CoverageThreshold=90

# Run specific test class
dotnet test --filter "ClassName=RateCurveBuilderTests"
```

### Test Organization

{{TEST_ORGANIZATION}}

**Current Coverage**: {{COVERAGE_PERCENT}}%

## Contributing

### Guidelines

1. **Fork the repository** and create a feature branch
2. **Write tests** for new functionality
3. **Ensure 90%+ coverage** for new code
4. **Follow SOLID principles** (see [SOLID Guidelines](.github/instructions/csharp-solid-principles.instructions.md))
5. **Update documentation** when adding features
6. **Submit a PR** with a clear description

### Development Workflow

```bash
# Create feature branch
git checkout -b feature/my-feature

# Make changes and run tests
dotnet test

# Commit with descriptive message
git commit -m "feat: add new curve building strategy"

# Push and create PR
git push origin feature/my-feature
```

### Code Review Checklist

- [ ] Tests pass (100% pass rate)
- [ ] Code coverage ≥ 90%
- [ ] SOLID principles followed
- [ ] Documentation updated
- [ ] No breaking changes (or documented)

## Documentation

- [SOLID Principles Guide](.github/instructions/csharp-solid-principles.instructions.md)
- [Testing Guide](.github/skills/quality-assurance/references/testing.md)
- [API Documentation](./docs/) (or link to generated docs)

## Version History

{{VERSION_HISTORY}}

## License

{{LICENSE}}

## Contact

{{CONTACT_INFO}}

---

**Last Updated**: {{LAST_UPDATED}}
**Maintained By**: {{MAINTAINERS}}
