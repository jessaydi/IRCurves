# Documentation Guide (Docfx & XML Comments)

## Overview

Documentation ensures API clarity and maintainability. Targets:
- ✅ All public types have XML doc comments
- ✅ All public methods have parameters documented
- ✅ No stale "TODO" or "FIXME" left in docs

## XML Documentation Comments

### Class Documentation

```csharp
/// <summary>
/// Builds interest rate term structures using specified curve building strategies.
/// </summary>
/// <remarks>
/// This class serves as a dispatcher for different curve building algorithms
/// (bootstrap, linear interpolation, Nelson-Siegel).
/// </remarks>
public class RateCurveBuilder
{
    // Implementation
}
```

### Method Documentation

```csharp
/// <summary>
/// Builds a yield curve from a collection of yield points.
/// </summary>
/// <param name="points">The yield points to use for curve construction.</param>
/// <param name="strategy">The curve building strategy to apply.</param>
/// <returns>
/// An InterestRateCurve object representing the constructed curve.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown if points or strategy is null.
/// </exception>
/// <example>
/// <code>
/// var points = new List&lt;YieldPoint&gt; { /* ... */ };
/// var curve = builder.Build(points);
/// </code>
/// </example>
public InterestRateCurve Build(List<YieldPoint> points, ICurveBuildingStrategy strategy)
{
    // Implementation
}
```

### Property Documentation

```csharp
/// <summary>
/// Gets the maturity date of the bond.
/// </summary>
/// <value>
/// A DateTime representing when the bond matures.
/// </value>
public DateTime MaturityDate { get; set; }
```

## Setting up Docfx

### Install Docfx

```bash
dotnet tool install -g docfx
```

### Create docfx.json

```json
{
  "build": {
    "content": [
      {
        "files": ["**/*.csproj"],
        "src": "TermStructure"
      }
    ],
    "dest": "_site",
    "globalMetadataFiles": [],
    "fileMetadataFiles": [],
    "overwrite": [],
    "externalReferences": [],
    "globalMetadata": {
      "_appTitle": "TermStructure API Documentation"
    }
  }
}
```

## Building Documentation

### Generate Docs

```bash
docfx docfx.json --serve
```

Opens documentation at `http://localhost:8080`

### Validate Documentation

```bash
docfx docfx.json --warningFilters docs.warnings.txt
```

## Documentation Checklist

- [ ] All `public class` has summary
- [ ] All `public method` has summary + param docs
- [ ] All `public property` has summary + value docs
- [ ] All `return` types documented
- [ ] All `exception` types documented
- [ ] At least one `<example>` per complex method
- [ ] No unresolved `<see cref="">` links
- [ ] No TODO/FIXME comments in production code

## Common Issues

| Issue | Fix |
|-------|-----|
| `cref` links broken | Ensure fully qualified: `<see cref="MyNamespace.MyClass.MyMethod"/>` |
| Generated docs empty | Check `docfx.json` paths match project layout |
| See Also links 404 | Verify links use correct namespace paths |

## Integration with CI/CD

Add to your QA pipeline:

```bash
# Generate docs (fail if warnings)
docfx docfx.json --warningsAsErrors
```

## References

- [Docfx Documentation](https://dotnet.github.io/docfx/)
- [XML Documentation Comments (C#)](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
