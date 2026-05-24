---
description: "Use when writing or reviewing C# code. Enforces SOLID principles (Single Responsibility, Interface Segregation, Dependency Inversion) to maintain clean, maintainable, and testable code."
applyTo: "**/*.cs"
name: "SOLID Principles for C#"
---

# SOLID Principles for C# (TermStructure Project)

This project follows SOLID design principles to ensure clean, maintainable, and testable code. Apply these guidelines when creating or modifying C# files.

## Single Responsibility Principle (SRP)

**One reason to change**: Each class should have a single, well-defined responsibility.

✅ **Do:**
- `RateCurveBuilder` handles rate curve construction logic only
- `FredDataFetcher` handles only data fetching from FRED API
- `InterestRate` model handles only interest rate representation and basic calculations
- Separate service classes from builders from models

❌ **Avoid:**
- Mixing API calls with business logic in the same class
- Having a "Util" or "Helper" class with random methods
- Models doing too much: parsing, validation, transformation, and serialization together

**Example:**
```csharp
// ✅ Good: Focused responsibility
public class FredDataFetcher : IMarketDataFetcher
{
    public async Task<List<YieldPoint>> FetchYieldDataAsync(string seriesId)
    {
        // Only handles data fetching
    }
}

// ❌ Avoid: Mixed responsibilities
public class DataManager
{
    public List<YieldPoint> FetchAndProcessAndTransformData() { }
}
```

---

## Interface Segregation Principle (ISP)

**Many focused interfaces over one large interface**: Clients should depend on small, specific interfaces they actually use.

✅ **Do:**
- Split large interfaces into smaller, focused ones: `ICurveBuildingStrategy`, `IInterpolationStrategy`
- Use separate interfaces for different concerns: `IMarketDataFetcher`, `IInterpolationStrategy`
- Create interfaces that represent a single capability

❌ **Avoid:**
- Large "god interfaces" like `IDataService` with 20 methods a client never uses
- Forcing implementations to implement methods they don't need

**Example:**
```csharp
// ✅ Good: Segregated interfaces
public interface ICurveBuildingStrategy
{
    InterestRateCurve Build(List<YieldPoint> points);
}

public interface IInterpolationStrategy
{
    double Interpolate(double x);
}

// ❌ Avoid: Fat interface
public interface IAllTheThings
{
    InterestRateCurve BuildCurve(List<YieldPoint> points);
    double Interpolate(double x);
    BondQuote FetchBond(string id);
}
```

---

## Dependency Inversion Principle (DIP)

**Depend on abstractions, not concrete implementations**: High-level modules should depend on interfaces/abstractions, not low-level details.

✅ **Do:**
- Use dependency injection to provide implementations (constructors or properties)
- Depend on interfaces and abstract classes
- Use the dispatcher/factory pattern (`CurveBuilderDispatcher`) to manage implementations

❌ **Avoid:**
- `new` operator to instantiate dependencies directly inside methods
- Tight coupling to concrete implementations
- Static dependencies

**Example:**
```csharp
// ✅ Good: Depends on abstraction
public class CurveBuilderDispatcher
{
    private readonly ICurveBuildingStrategy _strategy;
    
    public CurveBuilderDispatcher(ICurveBuildingStrategy strategy)
    {
        _strategy = strategy; // Injected dependency
    }
    
    public InterestRateCurve Build(List<YieldPoint> points)
    {
        return _strategy.Build(points);
    }
}

// ❌ Avoid: Tight coupling
public class CurveBuilderDispatcher
{
    public InterestRateCurve Build(List<YieldPoint> points)
    {
        var strategy = new BootstrapCurveBuildingStrategy(); // Concrete dependency
        return strategy.Build(points);
    }
}
```

---

## Project Structure Alignment

Your codebase already follows good patterns:

| Folder | Pattern | Principle |
|--------|---------|-----------|
| `Models/` | Data transfer objects | SRP: Only represent data |
| `Services/` | Data access and external API calls | SRP + DIP: Abstract via interfaces |
| `Builders/` | Construction logic with strategy dispatch | SRP + DIP: Delegate to strategies |
| `Strategies/` | Pluggable algorithms | ISP + DIP: Small, focused interfaces |

## Implementation Checklist

- [ ] Does this class have one, clear responsibility?
- [ ] If I need to change business logic, will I only modify one class?
- [ ] Is this class implementing only the methods it needs from its interfaces?
- [ ] Are dependencies injected (constructor or property) or retrieved via DIP?
- [ ] Could another developer understand this class's purpose in 30 seconds?
- [ ] Are there smaller interfaces instead of one large one?

---

## Common Violations to Avoid

1. **God Object**: A single class doing everything (builder + service + model)
2. **Feature Envy**: A class using too many methods from another class
3. **Hard Dependencies**: Creating objects with `new` instead of injecting them
4. **Fat Interfaces**: Classes forced to implement methods they don't use
5. **Mixing Concerns**: UI logic mixed with business logic, parsing mixed with transformations

---

## References

- **Single Responsibility**: Each class should have one reason to change
- **Interface Segregation**: Clients shouldn't depend on interfaces they don't use
- **Dependency Inversion**: Depend on abstractions, not concretions
