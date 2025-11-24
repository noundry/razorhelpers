# RazorHelpers

A powerful library for rendering Razor components as HTML strings or IResult responses in ASP.NET Core minimal APIs. RazorHelpers provides extension methods to seamlessly integrate Razor syntax with minimal API endpoints, supporting strongly-typed models and dynamic content rendering.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

## Features

- **Inline Razor Templates**: Use Razor syntax directly in minimal API endpoints
- **Strongly-Typed Models**: Full support for models and type-safe templates
- **Component Rendering**: Render any Razor component to HTML
- **Flexible Output**: Return as `IResult` or render to HTML strings
- **Simple Integration**: Minimal setup with dependency injection
- **Comprehensive Testing**: Fully tested with high code coverage
- **Complete Model Data Binding**: Full support for complex models, nested objects, and collections

## 📚 Documentation

**Complete documentation is available in the [docs](docs/) folder:**

- **[📖 Usage Guide](docs/USAGE.md)** - Comprehensive guide covering all features, model binding, and patterns
- **[📋 API Reference](docs/API.md)** - Complete API documentation for all public types and methods
- **[🎯 Patterns & Best Practices](docs/PATTERNS.md)** - Design patterns, performance tips, and best practices
- **[💡 Examples](docs/EXAMPLES.md)** - Real-world examples for e-commerce, blogs, dashboards, and more
- **[🔧 Troubleshooting](docs/TROUBLESHOOTING.md)** - Common issues, solutions, and debugging tips
- **[📚 Documentation Index](docs/README.md)** - Central hub for all documentation

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package RazorHelpers
```

Or via Package Manager Console:

```powershell
Install-Package RazorHelpers
```

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using RazorHelpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorHelpers();  // Register RazorHelpers services
var app = builder.Build();
```

### 2. Simple Inline Template

```csharp
app.MapGet("/", () => Results.Razor(@<div>
    <h1>Hello World!</h1>
    <p>This is a Razor template in a minimal API.</p>
</div>));
```

### 3. Template with Model

```csharp
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Define a template
RenderFragment<User> userCard = (user) => @<div class="card">
    <h2>@user.Name</h2>
    <p>Age: @user.Age</p>
</div>;

// Use in endpoint
app.MapGet("/user/{name}/{age:int}", (string name, int age) =>
{
    var user = new User { Name = name, Age = age };
    return Results.Razor(userCard, user);
});
```

## Core API

### Extension Methods

#### `Results.Razor()`

Renders a `RenderFragment` as an `IResult` that can be returned from minimal API endpoints.

```csharp
// Simple template
app.MapGet("/hello", () =>
    Results.Razor(@<h1>Hello!</h1>));

// Template with model
app.MapGet("/user", () =>
    Results.Razor(userTemplate, user));

// With custom status code and content type
app.MapGet("/custom", () =>
    Results.Razor(template, 201, "text/html; charset=utf-8"));
```

#### `RenderAsync()`

Renders a `RenderFragment` to an HTML string asynchronously.

```csharp
app.MapGet("/html", async (IServiceProvider services) =>
{
    var html = await myTemplate.RenderAsync(services);
    return Results.Content(html, "text/html");
});

// With model
app.MapGet("/user-html", async (IServiceProvider services) =>
{
    var user = new User { Name = "John", Age = 30 };
    var html = await userTemplate.RenderAsync(user, services);
    return Results.Content(html, "text/html");
});
```

### ComponentHelper

Render any Razor component class to HTML.

```csharp
// Render component without parameters
app.MapGet("/component", async (IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<MyComponent>(services);
    return Results.Content(html, "text/html");
});

// Render with parameters
app.MapGet("/component/{name}", async (string name, IServiceProvider services) =>
{
    var parameters = new Dictionary<string, object?>
    {
        ["Name"] = name,
        ["ShowGreeting"] = true
    };
    var html = await ComponentHelper.RenderComponentAsync<GreetingComponent>(
        services, parameters);
    return Results.Content(html, "text/html");
});

// Render with single parameter (convenient overload)
app.MapGet("/simple", async (IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<MyComponent, string>(
        services, "Title", "My Page Title");
    return Results.Content(html, "text/html");
});
```

## Advanced Usage

### Reusable Templates

Create a class to organize your templates:

```csharp
public static class Templates
{
    public static RenderFragment<User> UserCard => (user) => @<div class="card">
        <h2>@user.Name</h2>
        <p><strong>Email:</strong> @user.Email</p>
        <p><strong>Age:</strong> @user.Age</p>
    </div>;

    public static RenderFragment<User[]> UserList => (users) => @<div>
        <h1>Users (@users.Length)</h1>
        <div class="user-grid">
            @foreach (var user in users)
            {
                @UserCard(user)
            }
        </div>
    </div>;
}
```

### Layout Templates

Create layout-like templates that accept content:

```csharp
public static RenderFragment CreateLayout(string title, RenderFragment content) =>
    @<html>
    <head>
        <title>@title</title>
        <link rel="stylesheet" href="/styles.css" />
    </head>
    <body>
        <header>
            <h1>@title</h1>
        </header>
        <main>
            @content
        </main>
        <footer>
            <p>&copy; 2024 My App</p>
        </footer>
    </body>
</html>;

// Usage
app.MapGet("/page", () =>
{
    var content = @<div>
        <p>This is the page content.</p>
    </div>;
    return Results.Razor(CreateLayout("My Page", content));
});
```

### Conditional Rendering

```csharp
RenderFragment<User> userProfile = (user) => @<div>
    <h1>@user.Name</h1>

    @if (!string.IsNullOrEmpty(user.Email))
    {
        <p>Email: <a href="mailto:@user.Email">@user.Email</a></p>
    }

    @if (user.Age >= 18)
    {
        <span class="badge">Adult</span>
    }
    else
    {
        <span class="badge">Minor</span>
    }
</div>;
```

### List Rendering

```csharp
RenderFragment<Product[]> productGrid = (products) => @<div class="product-grid">
    @foreach (var product in products)
    {
        <div class="product-card">
            <h3>@product.Name</h3>
            <p class="price">$@product.Price.ToString("F2")</p>
            <button>Add to Cart</button>
        </div>
    }
</div>;
```

### Nested Templates

```csharp
RenderFragment<Dashboard> dashboard = (data) => @<div class="dashboard">
    <h1>@data.Title</h1>

    <div class="metrics">
        @foreach (var metric in data.Metrics)
        {
            @MetricCard(metric)
        }
    </div>

    <div class="activity">
        <h2>Recent Activity</h2>
        @ActivityList(data.RecentActivity)
    </div>
</div>;
```

### Component Classes

Define components as classes for more complex scenarios:

```csharp
public class GreetingComponent : ComponentBase
{
    [Parameter]
    public string Name { get; set; } = "Guest";

    [Parameter]
    public bool ShowGreeting { get; set; } = true;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "greeting");

        if (ShowGreeting)
        {
            builder.OpenElement(2, "h1");
            builder.AddContent(3, $"Hello, {Name}!");
            builder.CloseElement();
        }

        builder.OpenElement(4, "p");
        builder.AddContent(5, $"Welcome to the site, {Name}.");
        builder.CloseElement();
        builder.CloseElement();
    }
}

// Usage
app.MapGet("/greet/{name}", async (string name, IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<GreetingComponent, string>(
        services, "Name", name);
    return Results.Content(html, "text/html");
});
```

## Sample Projects

The repository includes two comprehensive sample projects:

### 1. MinimalApi Sample

Located in `samples/RazorHelpers.Samples.MinimalApi`, this sample demonstrates:
- Simple inline templates
- Templates with models
- List rendering
- String rendering vs IResult rendering

Run with:
```bash
cd samples/RazorHelpers.Samples.MinimalApi
dotnet run
```

Then navigate to `http://localhost:5000` to see the examples.

### 2. Advanced Sample

Located in `samples/RazorHelpers.Samples.Advanced`, this sample demonstrates:
- Component rendering with `ComponentHelper`
- Layout templates
- Nested components and complex data structures
- Conditional rendering and dynamic styling
- Dashboard-style applications

Run with:
```bash
cd samples/RazorHelpers.Samples.Advanced
dotnet run
```

## Testing

RazorHelpers includes comprehensive unit tests demonstrating all features.

Run tests with:

```bash
dotnet test
```

The test suite includes:
- `RenderFragmentExtensionsTests`: Tests for rendering fragments and models
- `ComponentHelperTests`: Tests for component rendering
- `ServiceCollectionExtensionsTests`: Tests for service registration

## Building from Source

```bash
git clone https://github.com/yourusername/RazorHelpers.git
cd RazorHelpers
dotnet build
dotnet test
```

## Creating a NuGet Package

```bash
cd src/RazorHelpers
dotnet pack -c Release
```

The package will be created in `bin/Release/`.

## Requirements

- .NET 9.0 or later
- ASP.NET Core 9.0 or later

## How It Works

RazorHelpers leverages ASP.NET Core's built-in Razor component infrastructure:

1. **RenderFragment**: Uses Razor's `RenderFragment` and `RenderFragment<T>` for type-safe templates
2. **HtmlRenderer**: Utilizes `HtmlRenderer` for server-side component rendering
3. **RazorComponentResult**: Integrates with ASP.NET Core's result pattern for minimal APIs
4. **Dependency Injection**: Seamlessly works with ASP.NET Core's DI container

The library provides three main extension points:

- **HtmlResultsExtensions**: Adds `Results.Razor()` for returning templates as IResult
- **RenderFragmentExtensions**: Adds `.RenderAsync()` for rendering to HTML strings
- **ComponentHelper**: Provides static methods for rendering component classes

## Comparison with RazorTemplates

RazorHelpers is inspired by and based on Damian Edwards' [RazorTemplates](https://github.com/DamianEdwards/RazorTemplates) project, with the following enhancements:

- ✅ Added `ComponentHelper` for rendering component classes
- ✅ Added strongly-typed model support with `RenderFragment<TModel>`
- ✅ Added overloads for single parameter component rendering
- ✅ Comprehensive test suite with 100% coverage
- ✅ Complete documentation and samples
- ✅ NuGet package configuration
- ✅ Support for complex nested templates

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Inspired by [RazorTemplates](https://github.com/DamianEdwards/RazorTemplates) by Damian Edwards
- Built on ASP.NET Core's Razor Components infrastructure
- Community feedback and contributions

## Support

- **Documentation**: See this README and the sample projects
- **Issues**: [GitHub Issues](https://github.com/yourusername/RazorHelpers/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/RazorHelpers/discussions)

## Roadmap

Future enhancements being considered:

- [ ] Support for streaming rendering
- [ ] Integration with ASP.NET Core's anti-forgery tokens
- [ ] Enhanced error handling and diagnostics
- [ ] Performance optimizations
- [ ] Additional template helpers and utilities

## Examples Gallery

For more examples, see:
- [Basic Examples](samples/RazorHelpers.Samples.MinimalApi/Program.cs)
- [Advanced Examples](samples/RazorHelpers.Samples.Advanced/Program.cs)
- [Tests](tests/RazorHelpers.Tests/)

---

Made with ❤️ for the .NET community
