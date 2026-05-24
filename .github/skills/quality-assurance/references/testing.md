# Unit Testing Guide (xUnit)

## Project Structure

- **Test Project**: `TermStructure.Tests`
- **Test Files**: `*Test.cs`, `*Tests.cs`
- **Test Framework**: xUnit

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Project
```bash
dotnet test TermStructure.Tests
```

### Specific Test Class
```bash
dotnet test --filter "ClassName=MyClass"
```

### Watch Mode (Continuous)
```bash
dotnet watch test
```

### Verbose Output
```bash
dotnet test --logger="console;verbosity=detailed"
```

## Test Organization

Tests should follow the AAA pattern:

```csharp
[Fact]
public void CurveBuilder_WithValidPoints_ReturnsCurve()
{
    // Arrange
    var points = new List<YieldPoint> { /* ... */ };
    var builder = new RateCurveBuilder();
    
    // Act
    var curve = builder.Build(points);
    
    // Assert
    Assert.NotNull(curve);
    Assert.Equal(expected, curve.Value);
}
```

## Coverage Requirements

- **Overall**: 90% minimum
- **Core Libraries**: 95% (Builders, Strategies)
- **Models**: 85% minimum
- **Services**: 90% minimum
- **Exclude**: Test projects, generated code

## Common Issues

| Issue | Fix |
|-------|-----|
| Tests not discoverable | Name files `*Test.cs` or `*Tests.cs` |
| Tests timeout | Add `[Fact(Timeout = 5000)]` attribute |
| Mock dependencies | Use `FakeHttpHandler` in TermStructure.Tests |
| Async test failures | Use `async Task` with `[Fact]` |

## Best Practices

1. **Test one thing per test** — Clear failure messages
2. **Use descriptive names** — `Method_Scenario_ExpectedResult`
3. **Avoid test interdependence** — Each test must run independently
4. **Mock external calls** — Never hit real APIs in tests
5. **Test edge cases** — Null inputs, empty collections, boundaries
