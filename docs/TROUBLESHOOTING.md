# RazorHelpers Troubleshooting Guide

Common issues, solutions, and debugging tips for RazorHelpers.

## Table of Contents

- [Installation Issues](#installation-issues)
- [Service Registration Issues](#service-registration-issues)
- [Rendering Issues](#rendering-issues)
- [Model Binding Issues](#model-binding-issues)
- [Performance Issues](#performance-issues)
- [Common Errors](#common-errors)
- [Debugging Tips](#debugging-tips)

## Installation Issues

### Issue: Package not found

**Problem**: `dotnet add package RazorHelpers` fails with package not found error.

**Solution**:
1. Ensure you're targeting .NET 9.0 or later in your `.csproj`:
   ```xml
   <TargetFramework>net9.0</TargetFramework>
   ```

2. Clear NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

3. Verify NuGet source:
   ```bash
   dotnet nuget list source
   ```

### Issue: Version conflict

**Problem**: Error about conflicting package versions.

**Solution**:
```xml
<!-- In your .csproj, explicitly specify the framework reference -->
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

## Service Registration Issues

### Issue: "Unable to resolve service for type 'ILoggerFactory'"

**Problem**: Application throws exception when trying to render a template.

**Symptoms**:
```
System.InvalidOperationException: Unable to resolve service for type
'Microsoft.Extensions.Logging.ILoggerFactory'
```

**Solution**:
Make sure you've registered RazorHelpers services:

```csharp
// ❌ Missing service registration
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => RazorResults.Razor(template));

// ✅ Correct
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorHelpers();  // Add this line!
var app = builder.Build();
app.MapGet("/", () => RazorResults.Razor(template));
```

### Issue: "Unable to resolve service for type 'IConfiguration'"

**Problem**: Error when rendering components that need configuration.

**Solution**:
ASP.NET Core automatically registers IConfiguration, but if you're in a minimal setup:

```csharp
builder.Services.AddSingleton<IConfiguration>(
    new ConfigurationBuilder().Build()
);
```

### Issue: "Unable to resolve service for type 'IWebHostEnvironment'"

**Problem**: Components trying to access web host environment fail.

**Solution**:
```csharp
// Register a test/mock environment
builder.Services.AddSingleton<IWebHostEnvironment>(
    new MockWebHostEnvironment()
);
```

## Rendering Issues

### Issue: Template renders as empty string

**Problem**: Template executes but produces no output.

**Diagnosis**:
```csharp
// Debug by checking intermediate values
RenderFragment template = builder =>
{
    Console.WriteLine("Template is executing");  // Add debug output
    builder.OpenElement(0, "div");
    builder.AddContent(1, "Test");
    builder.CloseElement();
};
```

**Common causes**:
1. **Forgot to close elements**:
   ```csharp
   // ❌ Missing CloseElement()
   builder.OpenElement(0, "div");
   builder.AddContent(1, "Text");
   // Missing: builder.CloseElement();

   // ✅ Correct
   builder.OpenElement(0, "div");
   builder.AddContent(1, "Text");
   builder.CloseElement();
   ```

2. **Early return in template**:
   ```csharp
   RenderFragment<User?> template = user => builder =>
   {
       if (user == null)
           return;  // Returns before rendering anything

       // Add fallback rendering before return
       if (user == null)
       {
           builder.AddContent(0, "No user found");
           return;
       }
   };
   ```

### Issue: HTML not rendering correctly

**Problem**: Output HTML is malformed or incomplete.

**Solution**: Check element nesting and sequence numbers:

```csharp
// ❌ Bad: Reused sequence numbers
RenderFragment bad = builder =>
{
    builder.OpenElement(0, "div");
    builder.OpenElement(0, "p");  // ❌ Same sequence number!
    builder.CloseElement();
    builder.CloseElement();
};

// ✅ Good: Unique sequence numbers
RenderFragment good = builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.OpenElement(seq++, "p");
    builder.CloseElement();
    builder.CloseElement();
};
```

### Issue: Content not HTML-encoded

**Problem**: User input showing as raw HTML or causing XSS vulnerabilities.

**Solution**: Use `AddContent` instead of `AddMarkupContent`:

```csharp
// ✅ Safe: Automatically encodes
builder.AddContent(0, userInput);  // <script> becomes &lt;script&gt;

// ❌ Dangerous: No encoding
builder.AddMarkupContent(0, userInput);  // <script> executes!

// ✅ Safe with sanitization
var sanitized = HtmlSanitizer.Sanitize(userInput);
builder.AddMarkupContent(0, sanitized);
```

### Issue: Razor syntax not working

**Problem**: Using `@<>` syntax gives compiler errors.

**Explanation**: RazorHelpers uses `RenderTreeBuilder` API, not Razor syntax.

```csharp
// ❌ This doesn't work in RazorHelpers
RenderFragment template = @<div>Hello</div>;

// ✅ Use RenderTreeBuilder API
RenderFragment template = builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, "Hello");
    builder.CloseElement();
};
```

## Model Binding Issues

### Issue: Model properties showing as default values

**Problem**: Model is null or properties are not binding correctly.

**Diagnosis**:
```csharp
RenderFragment<User> template = user => builder =>
{
    // Add debug output
    Console.WriteLine($"User: {user?.Name ?? "NULL"}");

    if (user == null)
    {
        builder.AddContent(0, "User is null!");
        return;
    }

    // Rest of template...
};
```

**Common causes**:

1. **Passing wrong parameter type**:
   ```csharp
   // ❌ Wrong: Passing RenderFragment<User> instead of User
   app.MapGet("/user", () =>
   {
       return RazorResults.Razor(userTemplate, userTemplate);  // Wrong!
   });

   // ✅ Correct
   app.MapGet("/user", () =>
   {
       var user = new User { Name = "John" };
       return RazorResults.Razor(userTemplate, user);
   });
   ```

2. **Null model not handled**:
   ```csharp
   // ✅ Always check for null
   RenderFragment<User?> template = user => builder =>
   {
       if (user == null)
       {
           builder.AddContent(0, "User not found");
           return;
       }

       // Safe to use user here
       builder.AddContent(1, user.Name);
   };
   ```

### Issue: Complex nested models not rendering

**Problem**: Properties of nested objects showing as null.

**Solution**: Ensure the entire object graph is populated:

```csharp
public class User
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();  // Initialize nested objects!
}

public class Address
{
    public string City { get; set; } = string.Empty;
}

// Always check nested objects
RenderFragment<User> template = user => builder =>
{
    builder.AddContent(0, user.Name);

    if (user.Address != null)  // Check before accessing
    {
        builder.AddContent(1, user.Address.City);
    }
};
```

### Issue: List/Collection not rendering items

**Problem**: Empty output when rendering collections.

**Diagnosis**:
```csharp
RenderFragment<List<Product>> template = products => builder =>
{
    Console.WriteLine($"Product count: {products?.Count ?? 0}");

    if (products == null || !products.Any())
    {
        builder.AddContent(0, "No products");
        return;
    }

    // Rest of template...
};
```

**Solution**:
```csharp
// ✅ Always check collection status
RenderFragment<List<Product>> template = products => builder =>
{
    var seq = 0;

    if (products == null || products.Count == 0)
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "No products available");
        builder.CloseElement();
        return;
    }

    foreach (var product in products)
    {
        builder.OpenElement(seq++, "div");
        builder.AddContent(seq++, product.Name);
        builder.CloseElement();
    }
};
```

## Performance Issues

### Issue: Slow rendering with large lists

**Problem**: Rendering thousands of items is slow.

**Solutions**:

1. **Implement pagination**:
   ```csharp
   app.MapGet("/products", (int page = 1, int pageSize = 50) =>
   {
       var products = GetProducts()
           .Skip((page - 1) * pageSize)
           .Take(pageSize)
           .ToList();

       return RazorResults.Razor(productListTemplate, products);
   });
   ```

2. **Use view models to reduce data**:
   ```csharp
   // ❌ Loading full entities
   var products = dbContext.Products.Include(p => p.Reviews).ToList();

   // ✅ Project to lightweight view model
   var products = dbContext.Products
       .Select(p => new ProductViewModel
       {
           Id = p.Id,
           Name = p.Name,
           Price = p.Price
       })
       .ToList();
   ```

3. **Cache rendered output**:
   ```csharp
   private static readonly MemoryCache _cache = new MemoryCache(
       new MemoryCacheOptions());

   app.MapGet("/products", async (IServiceProvider services) =>
   {
       var cacheKey = "products-html";

       if (_cache.TryGetValue(cacheKey, out string? cachedHtml))
       {
           return Results.Content(cachedHtml, "text/html");
       }

       var products = GetProducts();
       var html = await productTemplate.RenderAsync(products, services);

       _cache.Set(cacheKey, html, TimeSpan.FromMinutes(5));

       return Results.Content(html, "text/html");
   });
   ```

### Issue: Memory leaks with templates

**Problem**: Memory usage grows over time.

**Solution**: Use static readonly fields for templates:

```csharp
// ❌ Bad: Creates new template on each request
app.MapGet("/user", () =>
{
    RenderFragment<User> template = user => builder => { /* ... */ };
    return RazorResults.Razor(template, user);
});

// ✅ Good: Reuses same template
private static readonly RenderFragment<User> _userTemplate =
    user => builder => { /* ... */ };

app.MapGet("/user", () =>
{
    return RazorResults.Razor(_userTemplate, user);
});
```

## Common Errors

### Error: "Sequence numbers must be unique"

**Problem**:
```
System.InvalidOperationException: Sequence numbers must be unique and
should be generated in program source order
```

**Solution**: Use a counter for sequence numbers:

```csharp
// ✅ Correct
RenderFragment template = builder =>
{
    var seq = 0;  // Counter
    builder.OpenElement(seq++, "div");
    builder.AddContent(seq++, "Text");
    if (condition)
    {
        builder.OpenElement(seq++, "span");
        builder.CloseElement();
    }
    builder.CloseElement();
};
```

### Error: "The render tree frame at position X is of type Element"

**Problem**: Mismatched open/close element calls.

**Solution**: Ensure every `OpenElement` has a matching `CloseElement`:

```csharp
RenderFragment template = builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    // ... content ...
    builder.CloseElement();  // Must match OpenElement
};
```

### Error: "Object reference not set to an instance of an object"

**Problem**: Accessing null object in template.

**Solution**: Add null checks:

```csharp
RenderFragment<User> template = user => builder =>
{
    // ✅ Safe null checks
    builder.AddContent(0, user?.Name ?? "Unknown");
    builder.AddContent(1, user?.Email ?? "No email");

    if (user?.Address != null)
    {
        builder.AddContent(2, user.Address.City);
    }
};
```

## Debugging Tips

### Enable Detailed Logging

```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Inspect Rendered HTML

```csharp
app.MapGet("/debug", async (IServiceProvider services) =>
{
    var html = await myTemplate.RenderAsync(model, services);

    // Log the output
    Console.WriteLine("Rendered HTML:");
    Console.WriteLine(html);

    return Results.Content(html, "text/html");
});
```

### Use Try-Catch in Templates

```csharp
RenderFragment<User> safeTemplate = user => builder =>
{
    try
    {
        // Template logic
        builder.OpenElement(0, "div");
        builder.AddContent(1, user.Name);
        builder.CloseElement();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in template: {ex.Message}");
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "error");
        builder.AddContent(2, $"Error: {ex.Message}");
        builder.CloseElement();
    }
};
```

### Validate Models Before Rendering

```csharp
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);

    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    // Validate model
    if (string.IsNullOrEmpty(user.Name))
    {
        return Results.BadRequest("Invalid user data");
    }

    return RazorResults.Razor(userTemplate, user);
});
```

### Test Templates in Isolation

```csharp
[Fact]
public async Task TestUserTemplate()
{
    // Arrange
    var services = TestServiceProvider.Create();
    var user = new User
    {
        Name = "Test User",
        Email = "test@example.com"
    };

    // Act
    var html = await userTemplate.RenderAsync(user, services);

    // Assert
    Assert.Contains("Test User", html);
    Assert.Contains("test@example.com", html);
    Assert.DoesNotContain("null", html.ToLower());

    // Validate HTML structure
    Assert.Contains("<div", html);
    Assert.Equal(
        html.Count(c => c == '<' && html[html.IndexOf(c) + 1] != '/'),
        html.Count(c => c == '<' && html[html.IndexOf(c) + 1] == '/')
    );
}
```

### Create Debug Helper

```csharp
public static class DebugHelpers
{
    public static RenderFragment<T> WithDebug<T>(
        RenderFragment<T> template,
        string templateName)
    {
        return model => builder =>
        {
            Console.WriteLine($"[{templateName}] Rendering with model: {model?.GetType().Name}");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                builder.AddContent(0, template(model));
                Console.WriteLine($"[{templateName}] Rendered successfully in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{templateName}] Error: {ex.Message}");
                throw;
            }
        };
    }
}

// Usage
var debugTemplate = DebugHelpers.WithDebug(userTemplate, "UserCard");
```

## Getting Help

If you're still experiencing issues:

1. **Check the examples**: Review [Examples](EXAMPLES.md) for similar use cases
2. **Review patterns**: Check [Patterns](PATTERNS.md) for best practices
3. **Simplify**: Create a minimal reproduction of your issue
4. **GitHub Issues**: Report bugs at https://github.com/yourusername/RazorHelpers/issues

When reporting issues, include:
- .NET version (`dotnet --version`)
- RazorHelpers version
- Minimal code sample that reproduces the issue
- Expected vs actual behavior
- Full error message and stack trace

---

## See Also

- [Usage Guide](USAGE.md)
- [API Reference](API.md)
- [Patterns and Best Practices](PATTERNS.md)
- [Examples](EXAMPLES.md)
