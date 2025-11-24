# RazorHelpers Documentation

Complete documentation for RazorHelpers - a library for rendering Razor components in ASP.NET Core minimal APIs with full model data binding support.

## Quick Links

- **[Installation & Quick Start](../README.md#installation)** - Get started in 5 minutes
- **[Usage Guide](USAGE.md)** - Comprehensive usage guide covering all features
- **[API Reference](API.md)** - Complete API documentation
- **[Patterns & Best Practices](PATTERNS.md)** - Design patterns and best practices
- **[Examples](EXAMPLES.md)** - Real-world examples and use cases
- **[Troubleshooting](TROUBLESHOOTING.md)** - Common issues and solutions

## Documentation Overview

### For Beginners

Start here if you're new to RazorHelpers:

1. **[Installation](../README.md#installation)** - Install the package
2. **[Quick Start](USAGE.md#quick-start)** - Create your first template
3. **[Basic Usage](USAGE.md#basic-usage)** - Learn the fundamentals
4. **[Examples](EXAMPLES.md)** - See real-world implementations

### For Intermediate Users

Once you're comfortable with the basics:

1. **[Model Data Binding](USAGE.md#model-data-binding)** - Work with strongly-typed models
2. **[Advanced Patterns](USAGE.md#advanced-patterns)** - Learn composition and conditional rendering
3. **[Patterns Guide](PATTERNS.md)** - Organize and structure your templates
4. **[API Reference](API.md)** - Understand all available methods

### For Advanced Users

Deep dive into advanced topics:

1. **[Performance Optimization](PATTERNS.md#performance-optimization)** - Optimize rendering performance
2. **[Testing Patterns](PATTERNS.md#testing-patterns)** - Test your templates
3. **[Security Best Practices](PATTERNS.md#security-best-practices)** - Secure your templates
4. **[Troubleshooting](TROUBLESHOOTING.md)** - Debug complex issues

## Core Concepts

### RenderFragment

A `RenderFragment` is the foundation of template rendering in RazorHelpers. It's a delegate that describes how to build UI content using a `RenderTreeBuilder`.

```csharp
RenderFragment myTemplate = builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, "Hello World");
    builder.CloseElement();
};
```

**Learn more**: [Core Concepts](USAGE.md#core-concepts)

### Model Data Binding

RazorHelpers provides full support for strongly-typed models:

```csharp
RenderFragment<User> userTemplate = user => builder =>
{
    builder.AddContent(0, user.Name);
    builder.AddContent(1, user.Email);
};

app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(userTemplate, user);
});
```

**Learn more**: [Model Data Binding](USAGE.md#model-data-binding)

### Template Organization

Organize templates in static classes for better maintainability:

```csharp
public static class Templates
{
    public static RenderFragment<User> UserCard => /* ... */;
    public static RenderFragment<Product> ProductCard => /* ... */;
}
```

**Learn more**: [Template Organization](PATTERNS.md#template-organization)

## Common Use Cases

### E-Commerce Applications
- Product catalogs with filtering
- Shopping carts
- Checkout flows
- Order confirmations

**See**: [E-Commerce Examples](EXAMPLES.md#e-commerce-examples)

### Blogs and CMS
- Blog post rendering
- Comment systems
- Content lists
- Search results

**See**: [Blog and CMS Examples](EXAMPLES.md#blog-and-cms-examples)

### Dashboards and Admin Panels
- Analytics dashboards
- Data visualization
- Admin tables
- Reports

**See**: [Dashboard Examples](EXAMPLES.md#dashboard-and-admin-examples)

### Email Templates
- Transactional emails
- Order confirmations
- Newsletters
- Notifications

**See**: [Email Templates](EXAMPLES.md#email-templates)

### PDF Reports
- Sales reports
- Invoices
- Statements
- Analytics exports

**See**: [Report Generation](EXAMPLES.md#report-generation)

## API Overview

### RazorResults
Static helper for creating IResult responses:

```csharp
RazorResults.Razor(fragment)
RazorResults.Razor(fragment, model)
RazorResults.Razor(fragment, statusCode, contentType)
```

**See**: [RazorResults API](API.md#razorresults)

### RenderFragmentExtensions
Extension methods for rendering to strings:

```csharp
await fragment.RenderAsync(services)
await fragment.RenderAsync(model, services)
```

**See**: [RenderFragmentExtensions API](API.md#renderfragmentextensions)

### ComponentHelper
Helper for rendering component classes:

```csharp
await ComponentHelper.RenderComponentAsync<TComponent>(services)
await ComponentHelper.RenderComponentAsync<TComponent>(services, parameters)
```

**See**: [ComponentHelper API](API.md#componenthelper)

### ServiceCollectionExtensions
Service registration:

```csharp
builder.Services.AddRazorHelpers()
```

**See**: [ServiceCollectionExtensions API](API.md#servicecollectionextensions)

## Feature Matrix

| Feature | Supported | Documentation |
|---------|-----------|---------------|
| Basic Templates | ✅ | [Usage Guide](USAGE.md#basic-usage) |
| Model Binding | ✅ | [Model Binding](USAGE.md#model-data-binding) |
| Nested Models | ✅ | [Nested Models](USAGE.md#nested-model-binding) |
| Collections/Lists | ✅ | [List Rendering](USAGE.md#list-rendering) |
| Conditional Rendering | ✅ | [Conditional](USAGE.md#conditional-rendering) |
| Template Composition | ✅ | [Composition](PATTERNS.md#composition-patterns) |
| Component Classes | ✅ | [ComponentHelper](API.md#componenthelper) |
| Async Rendering | ✅ | [RenderAsync](API.md#renderasync) |
| Custom Status Codes | ✅ | [Configuration](USAGE.md#custom-http-status-codes) |
| Custom Content Types | ✅ | [Configuration](USAGE.md#custom-content-types) |
| HTML Encoding | ✅ (Automatic) | [Security](PATTERNS.md#html-encoding) |
| Layout Templates | ✅ | [Layouts](PATTERNS.md#layout-wrapper) |
| Email Generation | ✅ | [Email Examples](EXAMPLES.md#email-templates) |
| PDF Generation | ✅ | [PDF Examples](EXAMPLES.md#report-generation) |

## Performance Tips

1. **Cache templates as static readonly fields** - [Learn more](PATTERNS.md#template-precompilation)
2. **Use view models instead of domain models** - [Learn more](PATTERNS.md#view-model-transformation)
3. **Implement pagination for large lists** - [Learn more](TROUBLESHOOTING.md#slow-rendering-with-large-lists)
4. **Cache rendered output** - [Learn more](TROUBLESHOOTING.md#performance-issues)

## Security Guidelines

1. **Always use AddContent for user input** - Auto-encodes HTML
2. **Sanitize HTML before using AddMarkupContent** - Prevents XSS
3. **Validate URLs** - Prevents open redirects
4. **Check for null models** - Prevents null reference exceptions

**See**: [Security Best Practices](PATTERNS.md#security-best-practices)

## Testing Your Templates

```csharp
[Fact]
public async Task UserTemplate_RendersCorrectly()
{
    var services = TestServiceProvider.Create();
    var user = new User { Name = "Test" };

    var html = await Templates.UserCard.RenderAsync(user, services);

    Assert.Contains("Test", html);
}
```

**See**: [Testing Patterns](PATTERNS.md#testing-patterns)

## Frequently Asked Questions

### Can I use Razor syntax (@<>) with RazorHelpers?
No, RazorHelpers uses the `RenderTreeBuilder` API. See [Usage Guide](USAGE.md#core-concepts) for syntax.

### How do I handle null models?
Always check for null in your templates. See [Nullable Model Handling](PATTERNS.md#nullable-model-handling).

### Can I use dependency injection in templates?
Templates can't directly inject services, but you can pass them as parameters. See [API Reference](API.md#dependency-injection-in-templates).

### How do I optimize performance?
Cache templates, use view models, and implement pagination. See [Performance Optimization](PATTERNS.md#performance-optimization).

### Is HTML automatically encoded?
Yes, when using `AddContent`. Use `AddMarkupContent` for trusted HTML only. See [Security](PATTERNS.md#html-encoding).

## Version History

### Version 1.0.0 (Current)
- Initial release
- Support for RenderFragment and RenderFragment<T>
- Model data binding
- ComponentHelper for rendering component classes
- Comprehensive documentation

## Contributing

Contributions are welcome! See the main [README](../README.md#contributing) for guidelines.

## License

MIT License - See [LICENSE](../LICENSE) for details.

## Support

- **GitHub Issues**: https://github.com/yourusername/RazorHelpers/issues
- **Documentation**: https://github.com/yourusername/RazorHelpers/tree/main/docs

---

**Next Steps**: Start with the [Usage Guide](USAGE.md) or explore [Examples](EXAMPLES.md)!
