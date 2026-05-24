# STRUCTURE.md — TermStructure Project Organization

## Directory Layout (Source Only)

```
IRCurves/                              # Repository root
│
├── TermStructure/                     # Main library project (.NET class library)
│   ├── TermStructure.csproj
│   ├── Builders/                      # Template method pattern implementations
│   │   ├── RateCurveBuilder.cs        # Abstract base (template method)
│   │   ├── YieldCurveBuilder.cs       # Concrete yield curve builder
│   │   └── CurveBuilderDispatcher.cs  # Factory/dispatcher for strategy selection
│   ├── Services/                      # External integration layer
│   │   ├── MarketDataFetcher.cs       # Abstract base for data fetching
│   │   └── FredDataFetcher.cs         # FRED API client
│   ├── Strategies/                    # Pluggable algorithm strategies
│   │   ├── BuildingStrategies/
│   │   │   ├── ICurveBuildingStrategy.cs              # Strategy interface
│   │   │   ├── BootstrapCurveBuildingStrategy.cs      # Bootstrap method
│   │   │   ├── LinearCurveBuildingStrategy.cs         # Linear interpolation builder
│   │   │   └── NelsonSiegelCurveBuildingStrategy.cs   # 4-factor Nelson-Siegel
│   │   └── InterpolationStrategies/
│   │       ├── IInterpolationStrategy.cs              # Strategy interface
│   │       └── LinearInterpolationStrategy.cs         # Linear interpolation
│   └── Models/                        # Data transfer objects (DTOs)
│       ├── YieldPoint.cs              # (maturity, rate) tuple
│       ├── BondQuote.cs               # Bond market data
│       ├── InterestRate.cs            # Rate with tenor and type
│       ├── FredModels.cs              # FRED API response models
│       └── FredObservation.cs         # Time series observation
│
├── TermStructure.Tests/               # xUnit test project
│   ├── TermStructure.Tests.csproj
│   ├── UnitTest1.cs                   # FredDataTests (5 async tests)
│   └── RateCurveBuilderTests.cs        # RateCurveBuilderTests (26 comprehensive tests)
│
├── TermStructure.Tester/              # Console application (optional, entry point for manual testing)
│   ├── TermStructure.Tester.csproj
│   └── Program.cs
│
├── .github/                           # GitHub-specific config
│   ├── workflows/                     # CI/CD pipelines (GitHub Actions)
│   ├── agents/                        # Copilot custom agents
│   │   └── test-expert.agent.md
│   ├── instructions/                  # Coding standards
│   │   └── csharp-solid-principles.instructions.md
│   ├── skills/                        # Reusable workflows
│   │   ├── quality-assurance/
│   │   └── project-documentation/
│   └── ISSUE_TEMPLATE/
│
├── .agents/                           # External agent skills
│   └── skills/
│       └── acquire-codebase-knowledge/
│
├── docs/                              # Documentation
│   └── codebase/                      # Codebase knowledge files (this directory)
│
├── README.md                          # Project README (comprehensive)
└── .vscode/                           # VS Code configuration
    └── mcp.json                       # Model Context Protocol config
```

---

## File Organization by Layer

### Entry Points

| File | Type | Purpose |
|------|------|---------|
| `TermStructure.Tester/Program.cs` | Console App | Manual testing / example entry point |
| `TermStructure/Builders/CurveBuilderDispatcher.cs` | Factory | Programmatic entry point for curve building |

**[TODO]** Main entry point unclear — `TermStructure.Tester/Program.cs` not analyzed in detail.

---

### Core Application Layer (`TermStructure/`)

**Responsibility**: Interest rate curve building and interpolation

| Folder | Files | Purpose |
|--------|-------|---------|
| `Builders/` | 3 files | Orchestrate curve construction (template method pattern) |
| `Strategies/` | 6 files | Pluggable algorithms for building and interpolation |
| `Services/` | 2 files | External API integration (FRED) |
| `Models/` | 5 files | Data structures and DTOs |

---

### Test Layer (`TermStructure.Tests/`)

| File | Tests | Purpose |
|------|-------|---------|
| `UnitTest1.cs` | 5 (async) | FRED API client testing |
| `RateCurveBuilderTests.cs` | 26 | Curve builder and strategy testing |
| `FakeHttpHandler.cs` | — | Mock HTTP handler for testing |

**Total**: 31 unit tests, 90%+ coverage target

---

## Key Files & Their Roles

### Public Interfaces (Strategy Pattern)

```csharp
TermStructure/Strategies/BuildingStrategies/ICurveBuildingStrategy.cs
  ↓ Implemented by:
  - BootstrapCurveBuildingStrategy.cs
  - LinearCurveBuildingStrategy.cs
  - NelsonSiegelCurveBuildingStrategy.cs

TermStructure/Strategies/InterpolationStrategies/IInterpolationStrategy.cs
  ↓ Implemented by:
  - LinearInterpolationStrategy.cs
```

### Abstract Base Classes (Template Method Pattern)

```csharp
TermStructure/Builders/RateCurveBuilder.cs (abstract)
  ↓ Inherited by:
  - YieldCurveBuilder.cs
```

### Data Models (DTOs)

```csharp
TermStructure/Models/
  ├── YieldPoint.cs         # (Maturity, Rate) pair
  ├── BondQuote.cs          # Bond market snapshot
  ├── InterestRate.cs       # Tenor + rate + type
  ├── FredModels.cs         # FRED response structures
  └── FredObservation.cs    # Time series point
```

### External Integration

```csharp
TermStructure/Services/
  ├── MarketDataFetcher.cs  # Abstract base
  └── FredDataFetcher.cs    # FRED REST client (async)
```

---

## Code Metrics

| Metric | Value | Evidence |
|--------|-------|----------|
| **Total C# Files** | 19 | Scan result |
| **Total Lines of Code** | 2,135 | Scan result |
| **Largest File** | `RateCurveBuilderTests.cs` | 19.3 KB |
| **Test Files** | 2 | `UnitTest1.cs`, `RateCurveBuilderTests.cs` |
| **Test Count** | 31 | Last terminal run |

---

## Build Output Locations

```
TermStructure/bin/Release/net9.0/
  └── TermStructure.dll                # Main library assembly

TermStructure.Tests/bin/Release/net9.0/
  └── TermStructure.Tests.dll          # Test assembly
```

---

## Dependencies Between Projects

```
TermStructure.Tests
  ↓ (references)
  TermStructure (main library)
  ↓ (depends on)
  System.Net.Http (FRED API)
  System.Text.Json (JSON parsing)

TermStructure.Tester
  ↓ (references)
  TermStructure (main library)
```

---

## Package Locations

- **NuGet packages**: Resolved to `~/.nuget/packages/` (standard NuGet cache)
- **Project dependencies**: Defined in `.csproj` `<ItemGroup><ProjectReference>` and `<ItemGroup><PackageReference>` sections

---

## Configuration Files

| File | Purpose | Status |
|------|---------|--------|
| `.csproj` files | Build configuration, dependencies | Present in each project |
| `.vscode/mcp.json` | Model Context Protocol | Present |
| `.github/workflows/` | CI/CD | Directory present, specific file not analyzed |
| `README.md` | Project documentation | 13.6 KB, comprehensive |

---

## [TODO] Items

- [ ] Actual content of `.github/workflows/` files not analyzed (directory structure present)
- [ ] `TermStructure.Tester/Program.cs` entry point not analyzed in detail
- [ ] No `.sln` (solution file) at root — using separate `.csproj` files

---

**Evidence Files**:
- Scan output: code tree and file listing
- `README.md` (describes module structure)
- Individual `.csproj` files (project references)
