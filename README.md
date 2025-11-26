# RazorHelpers

A powerful library for rendering Razor components as HTML strings or IResult responses in ASP.NET Core minimal APIs. RazorHelpers uses the RenderTreeBuilder API to enable programmatic template creation with full support for strongly-typed models and dynamic content rendering.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

## Features

- **🆕 HtmlBuilder**: Fluent, semantic HTML building API - `Html.Div().Class("card").Child(Html.H1("Title"))`
- **🆕 TableBuilder**: Generate tables from collections with `Html.Table(users).Column("Name", u => u.Name)`
- **🆕 SelectBuilder**: Build dropdowns with grouping via `Html.Select(items).GroupBy(x => x.Category)`
- **RenderFragment Templates**: Use RenderTreeBuilder API for programmatic template creation
- **Strongly-Typed Models**: Full support for `RenderFragment<T>` with type-safe model binding
- **Component Rendering**: Render any Razor component class to HTML
- **Flexible Output**: Return as `IResult` or render to HTML strings
- **Simple Integration**: Minimal setup with dependency injection
- **Comprehensive Testing**: Fully tested with high code coverage (81 passing tests)
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
using Microsoft.AspNetCore.Components.Rendering;

RenderFragment simpleTemplate = builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, "Hello World!");
    builder.CloseElement();
    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, "This is a Razor template in a minimal API.");
    builder.CloseElement();
    builder.CloseElement();
};

app.MapGet("/", () => RazorResults.Razor(simpleTemplate));
```

### 3. Template with Model

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

public class User
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Define a template
RenderFragment<User> userCard = user => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "card");
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, user.Name);
    builder.CloseElement();
    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Age: {user.Age}");
    builder.CloseElement();
    builder.CloseElement();
};

// Use in endpoint
app.MapGet("/user/{name}/{age:int}", (string name, int age) =>
{
    var user = new User { Name = name, Age = age };
    return RazorResults.Razor(userCard, user);
});
```

### 4. HtmlBuilder - Fluent HTML (New!)

Build HTML with a clean, semantic, fluent API instead of raw RenderTreeBuilder:

```csharp
// Simple elements
var fragment = Html.Div("Hello World").Class("greeting").Render();

// Nested elements
var card = Html.Div()
    .Class("card")
    .Style("padding", "20px")
    .Child(Html.H1("Welcome"))
    .Child(Html.P("This is so much cleaner!"))
    .Render();

app.MapGet("/card", () => RazorResults.Razor(card));
```

### 5. Tables from Collections

```csharp
var users = new[] {
    new User("John", "john@example.com", true),
    new User("Jane", "jane@example.com", false)
};

var table = Html.Table(users)
    .Class("table", "table-striped")
    .Column("Name", u => Html.Strong(u.Name))
    .Column("Email", u => Html.A($"mailto:{u.Email}", u.Email))
    .Column("Status", u => Html.Span(u.IsActive ? "Active" : "Inactive")
        .Style("color", u.IsActive ? "green" : "red"))
    .Render();

app.MapGet("/users", () => RazorResults.Razor(table));
```

### 6. Select Dropdowns from Collections

```csharp
var countries = new[] {
    new Country("us", "United States"),
    new Country("uk", "United Kingdom")
};

var select = Html.Select(countries, "country")
    .Placeholder("Select a country...")
    .Value(c => c.Code)
    .Text(c => c.Name)
    .SelectedValue("uk")
    .Render();

// With automatic grouping
var cars = GetCars(); // Volvo, Saab, Mercedes, Audi with Country property
var grouped = Html.Select(cars, "car")
    .Value(c => c.Code)
    .Text(c => c.Name)
    .GroupBy(c => c.Country)  // Creates <optgroup> automatically!
    .Render();
```

## Core API

### Extension Methods

#### `RazorResults.Razor()`

Renders a `RenderFragment` as an `IResult` that can be returned from minimal API endpoints.

```csharp
// Simple template
RenderFragment helloTemplate = builder =>
{
    builder.OpenElement(0, "h1");
    builder.AddContent(1, "Hello!");
    builder.CloseElement();
};

app.MapGet("/hello", () => RazorResults.Razor(helloTemplate));

// Template with model
app.MapGet("/user", () =>
{
    var user = new User { Name = "John", Age = 30 };
    return RazorResults.Razor(userTemplate, user);
});

// With custom status code and content type
app.MapGet("/custom", () =>
    RazorResults.Razor(template, 201, "text/html; charset=utf-8"));
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

### HtmlBuilder

Build HTML elements fluently with the `Html` static class:

```csharp
// All standard HTML elements
Html.Div(), Html.Span(), Html.P(), Html.H1() - Html.H6()
Html.A(href, text), Html.Img(src, alt), Html.Button(text)
Html.Form(action, method), Html.Input(type, name, value)
Html.Ul(), Html.Ol(), Html.Li(), Html.Table(), Html.Select()
Html.Strong(), Html.Em(), Html.B(), Html.I(), Html.U(), Html.S()
Html.Code(), Html.Pre(), Html.Kbd(), Html.Samp(), Html.Var(), Html.Q()
// ... and many more

// Fluent methods for all elements
.Id("my-id")                      // Set id attribute
.Class("class1", "class2")        // Add CSS classes
.ClassIf("active", isActive)      // Conditional class
.Style("color", "red")            // Inline styles
.Attr("data-id", "123")           // Any attribute
.Data("id", "123")                // data-* attributes
.Text("content")                  // Text content
.Raw("<b>html</b>")               // Raw HTML
.Child(element)                   // Add child element
.Children(elements)               // Add multiple children
.Render()                         // Convert to RenderFragment

// Lists from collections
Html.Ul(items, x => x.Name)                    // Simple text list
Html.Ul(items, x => Html.Strong(x.Name))       // Custom elements
Html.Each(items, x => Html.Div(x.Name))        // Any repeating element
```

### TableBuilder

```csharp
// Manual table
Html.Table()
    .Class("table")
    .Caption("Users")
    .Header("Name", "Email", "Status")
    .Row("John", "john@example.com", "Active")
    .Row("Jane", "jane@example.com", "Inactive")
    .Render();

// From collection with row selector
Html.Table(users)
    .Header("Name", "Email")
    .Row(u => [u.Name, u.Email])  // String array
    .Render();

// From collection with column definitions (recommended)
Html.Table(users)
    .Column("Name", u => u.Name)                           // Text column
    .Column("Email", u => Html.A($"mailto:{u.Email}", u.Email))  // Element column
    .Column("Status", u => u.IsActive ? "Active" : "Inactive")
    .RowClass(u => u.IsActive ? "active" : "inactive")     // Row CSS class
    .Render();
```

### SelectBuilder

```csharp
// Manual select
Html.Select("country")
    .Option("", "Select...")
    .Option("us", "United States")
    .Option("uk", "United Kingdom", selected: true)
    .OptGroup("Europe")
        .Option("de", "Germany")
        .Option("fr", "France")
    .EndGroup()
    .Render();

// From collection
Html.Select(countries, "country")
    .Placeholder("Select a country...")
    .Value(c => c.Code)
    .Text(c => c.Name)
    .SelectedValue("us")
    .DisabledOption(c => c.IsRestricted)
    .Render();

// With automatic grouping
Html.Select(cars, "car")
    .Value(c => c.Code)
    .Text(c => c.Name)
    .GroupBy(c => c.Manufacturer)  // Creates optgroups automatically
    .Render();
```

## Advanced Usage

### Reusable Templates

Create a class to organize your templates:

```csharp
public static class Templates
{
    public static RenderFragment<User> UserCard => user => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "card");
        builder.OpenElement(seq++, "h2");
        builder.AddContent(seq++, user.Name);
        builder.CloseElement();
        builder.OpenElement(seq++, "p");
        builder.OpenElement(seq++, "strong");
        builder.AddContent(seq++, "Email:");
        builder.CloseElement();
        builder.AddContent(seq++, " " + user.Email);
        builder.CloseElement();
        builder.OpenElement(seq++, "p");
        builder.OpenElement(seq++, "strong");
        builder.AddContent(seq++, "Age:");
        builder.CloseElement();
        builder.AddContent(seq++, " " + user.Age);
        builder.CloseElement();
        builder.CloseElement();
    };

    public static RenderFragment<User[]> UserList => users => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.OpenElement(seq++, "h1");
        builder.AddContent(seq++, $"Users ({users.Length})");
        builder.CloseElement();
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "user-grid");
        foreach (var user in users)
        {
            builder.AddContent(seq++, UserCard(user));
        }
        builder.CloseElement();
        builder.CloseElement();
    };
}
```

### Layout Templates

Create layout-like templates that accept content. For complete layout examples, see the [Patterns documentation](docs/PATTERNS.md#layout-wrapper).

### Conditional Rendering

```csharp
RenderFragment<User> userProfile = user => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, user.Name);
    builder.CloseElement();

    if (!string.IsNullOrEmpty(user.Email))
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "Email: ");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", $"mailto:{user.Email}");
        builder.AddContent(seq++, user.Email);
        builder.CloseElement();
        builder.CloseElement();
    }

    builder.OpenElement(seq++, "span");
    builder.AddAttribute(seq++, "class", "badge");
    builder.AddContent(seq++, user.Age >= 18 ? "Adult" : "Minor");
    builder.CloseElement();

    builder.CloseElement();
};
```

### List Rendering

```csharp
RenderFragment<Product[]> productGrid = products => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "product-grid");

    foreach (var product in products)
    {
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "product-card");
        builder.OpenElement(seq++, "h3");
        builder.AddContent(seq++, product.Name);
        builder.CloseElement();
        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "class", "price");
        builder.AddContent(seq++, $"${product.Price:F2}");
        builder.CloseElement();
        builder.OpenElement(seq++, "button");
        builder.AddContent(seq++, "Add to Cart");
        builder.CloseElement();
        builder.CloseElement();
    }

    builder.CloseElement();
};
```

For more comprehensive examples with complete implementations, see the [documentation](docs/) and [sample projects](samples/).

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
- **HtmlBuilder** - Fluent HTML building (`/html-builder`)
- **TableBuilder** - Tables from collections (`/html-builder/table`)
- **SelectBuilder** - Forms with selects (`/html-builder/form`)
- Card layouts with collections (`/html-builder/cards`)
- Nested components and complex data structures

Run with:
```bash
cd samples/RazorHelpers.Samples.Advanced
dotnet run
```

Then navigate to `http://localhost:5000/html-builder` to see the HtmlBuilder demos.

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

- [x] **HtmlBuilder** - Fluent HTML building API ✅ (v1.1.0)
- [x] **TableBuilder** - Table generation from collections ✅ (v1.1.0)
- [x] **SelectBuilder** - Select/dropdown generation ✅ (v1.1.0)
- [ ] Support for streaming rendering
- [ ] Integration with ASP.NET Core's anti-forgery tokens
- [ ] Enhanced error handling and diagnostics
- [ ] Performance optimizations

## Examples Gallery

For more examples, see:
- [Basic Examples](samples/RazorHelpers.Samples.MinimalApi/Program.cs)
- [Advanced Examples](samples/RazorHelpers.Samples.Advanced/Program.cs)
- [Tests](tests/RazorHelpers.Tests/)

---

Made with ❤️ for the .NET community
