# STACK.md — TermStructure Technology Stack

## Language & Runtime

- **Language**: C# (.NET ecosystem)
- **Runtime**: .NET 9.0
- **SDK**: Microsoft.NET.Sdk
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled for cleaner imports

**Evidence**: `TermStructure/TermStructure.csproj` and `TermStructure.Tests/TermStructure.Tests.csproj` both specify `net9.0`

---

## Core Framework & Libraries

### Production Dependencies

**Primary Libraries** (for `TermStructure/`):
- **None explicitly listed in .csproj** — Uses only built-in .NET libraries
  - `System.Net.Http` — HTTP client for FRED API integration
  - `System.Text.Json` — JSON deserialization for API responses
  - `System.Globalization` — Decimal parsing with culture info

**Evidence**: `TermStructure/TermStructure.csproj` has no `<ItemGroup><PackageReference>` sections

---

### Test & Development Dependencies

**Test Framework** (`TermStructure.Tests/`):
- **xUnit** v2.9.2 — Unit testing framework
- **Moq** v4.20.72 — Mocking library for dependency injection tests
- **Coverlet** v6.0.2 — Code coverage measurement
- **Microsoft.NET.Test.Sdk** v17.12.0 — Test infrastructure
- **xunit.runner.visualstudio** v2.8.2 — VS integration

**Evidence**: `TermStructure.Tests/TermStructure.Tests.csproj` `<ItemGroup><PackageReference>` sections

---

## Build & Compilation

| Configuration | Details |
|---|---|
| **Default Language Version** | Latest C# (.NET 9.0) |
| **Nullable Reference Types** | Enabled (non-nullable by default, nullability warnings enforced) |
| **Implicit Global Usings** | Enabled (reduces boilerplate in every file) |
| **Output Type** | Class Library (TermStructure) & Test Assembly (TermStructure.Tests) |

**Build Targets**:
```
dotnet build                           # Debug configuration
dotnet build --configuration Release   # Production optimized
```

---

## CI/CD Stack

**Provider**: GitHub Actions

**Files**:
- `.github/workflows/` — Workflow definitions (directory detected, specific file not analyzed)

**Evidence**: Scan detected `CI/CD: GitHub Actions` section

---

## External Integrations

### REST APIs

- **FRED (Federal Reserve Economic Data) API**
  - Endpoint: `https://api.stlouisfed.org/fred/`
  - Authentication: API key
  - Purpose: Fetch economic time series data (treasury yields, etc.)
  - Usage: `FredDataFetcher` class
  - Methods: GET `/series/observations`

**Evidence**: `TermStructure/Services/FredDataFetcher.cs` — Contains hardcoded FRED base URL and API integration

---

## Containerization & Deployment

**Status**: No containerization detected.

- No `Dockerfile` present
- No `docker-compose.yml` present
- No Kubernetes configs (`.yaml`, `.yml`)

**[ASK USER]** Are there plans to containerize? Should Docker support be added to the stack documentation?

---

## Code Quality & Tooling

### Linting & Formatting

**Status**: No dedicated linting/formatting configs detected.

- No `.editorconfig` file
- No StyleCop or other linting rules
- No Roslyn analyzers configured

**Note**: xUnit and Moq have built-in analyzer support (e.g., xUnit2002 warnings observed in builds)

**[ASK USER]** Should project adopt `.editorconfig` for consistency? Should StyleCop or EditorConfig be configured?

---

## Package Manager

**Dependency Management**: NuGet

- All packages pulled from `https://api.nuget.org/v3/` (official NuGet feed)
- Transitive dependencies resolved by `dotnet restore`

**Evidence**: Terminal output shows successful NuGet package restores

---

## Summary

| Category | Technology |
|----------|-----------|
| **Language** | C# |
| **Runtime** | .NET 9.0 |
| **Test Framework** | xUnit 2.9.2 + Moq 4.20.72 |
| **Coverage Tool** | Coverlet 6.0.2 |
| **CI/CD** | GitHub Actions |
| **External APIs** | FRED (HTTP REST) |
| **Package Manager** | NuGet |
| **Containerization** | None (yet) |

---

## [TODO] Items

- [ ] Specific GitHub Actions workflow file not analyzed (directory found but file not read)
- [ ] No build server agents or resource specifications documented
- [ ] No secret management strategy documented (API keys, etc.)

---

**Evidence Files**:
- `TermStructure/TermStructure.csproj`
- `TermStructure.Tests/TermStructure.Tests.csproj`
- `README.md` (describes FRED integration and test framework)
