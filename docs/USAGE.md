# RazorHelpers Usage Guide

Complete guide to using RazorHelpers for rendering Razor components in minimal APIs.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Basic Usage](#basic-usage)
- [Model Data Binding](#model-data-binding)
- [Advanced Patterns](#advanced-patterns)
- [Configuration](#configuration)

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

// Register RazorHelpers services
builder.Services.AddRazorHelpers();

var app = builder.Build();
```

### 2. Create Your First Template

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

// Define a simple template
RenderFragment simpleTemplate = builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, "Hello from RazorHelpers!");
    builder.CloseElement();
};

// Use it in an endpoint
app.MapGet("/", () => RazorResults.Razor(simpleTemplate));
```

### 3. Run Your App

```bash
dotnet run
```

Navigate to `http://localhost:5000` to see your rendered template!

## Core Concepts

### RenderFragment

A `RenderFragment` is a delegate that represents a segment of UI content. It's the foundation of Razor component rendering.

```csharp
// Basic RenderFragment
RenderFragment fragment = builder =>
{
    builder.OpenElement(0, "h1");
    builder.AddContent(1, "Hello World");
    builder.CloseElement();
};

// RenderFragment with model
RenderFragment<User> userTemplate = user => builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, $"Name: {user.Name}");
    builder.CloseElement();
};
```

### RenderTreeBuilder

The `RenderTreeBuilder` is used to construct the UI tree. Key methods:

- `OpenElement(sequence, elementName)` - Start an HTML element
- `CloseElement()` - Close the current element
- `AddContent(sequence, content)` - Add text content
- `AddAttribute(sequence, name, value)` - Add an attribute
- `AddMarkupContent(sequence, markup)` - Add raw HTML

**Important**: Sequence numbers should be unique and in ascending order within each scope.

## Basic Usage

### Simple Templates

```csharp
RenderFragment welcome = builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "welcome");

    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, "Welcome!");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, "This is a simple template.");
    builder.CloseElement();

    builder.CloseElement();
};

app.MapGet("/welcome", () => RazorResults.Razor(welcome));
```

### Templates with Attributes

```csharp
RenderFragment styledButton = builder =>
{
    builder.OpenElement(0, "button");
    builder.AddAttribute(1, "class", "btn btn-primary");
    builder.AddAttribute(2, "onclick", "alert('Clicked!')");
    builder.AddContent(3, "Click Me");
    builder.CloseElement();
};
```

### Templates with Multiple Elements

```csharp
RenderFragment navigation = builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "nav");
    builder.OpenElement(seq++, "ul");

    // First link
    builder.OpenElement(seq++, "li");
    builder.OpenElement(seq++, "a");
    builder.AddAttribute(seq++, "href", "/home");
    builder.AddContent(seq++, "Home");
    builder.CloseElement();
    builder.CloseElement();

    // Second link
    builder.OpenElement(seq++, "li");
    builder.OpenElement(seq++, "a");
    builder.AddAttribute(seq++, "href", "/about");
    builder.AddContent(seq++, "About");
    builder.CloseElement();
    builder.CloseElement();

    builder.CloseElement(); // ul
    builder.CloseElement(); // nav
};
```

## Model Data Binding

RazorHelpers provides full support for strongly-typed model data binding.

### Basic Model Binding

```csharp
// Define your model
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}

// Create a typed template
RenderFragment<Product> productCard = product => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "product-card");

    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, product.Name);
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddAttribute(seq++, "class", "price");
    builder.AddContent(seq++, $"${product.Price:F2}");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, product.Description);
    builder.CloseElement();

    builder.CloseElement();
};

// Use in endpoint
app.MapGet("/product/{id:int}", (int id) =>
{
    var product = new Product
    {
        Id = id,
        Name = "Sample Product",
        Price = 29.99m,
        Description = "A great product!"
    };

    return RazorResults.Razor(productCard, product);
});
```

### Complex Model Binding

```csharp
public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Price * i.Quantity);
}

public class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

RenderFragment<Order> orderTemplate = order => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "order");

    // Order header
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, $"Order #{order.OrderId}");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Customer: {order.CustomerName}");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Date: {order.OrderDate:yyyy-MM-dd}");
    builder.CloseElement();

    // Items table
    builder.OpenElement(seq++, "table");
    builder.OpenElement(seq++, "thead");
    builder.OpenElement(seq++, "tr");
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Product");
    builder.CloseElement();
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Quantity");
    builder.CloseElement();
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Price");
    builder.CloseElement();
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "tbody");
    foreach (var item in order.Items)
    {
        builder.OpenElement(seq++, "tr");
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, item.ProductName);
        builder.CloseElement();
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, item.Quantity.ToString());
        builder.CloseElement();
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, $"${item.Price:F2}");
        builder.CloseElement();
        builder.CloseElement();
    }
    builder.CloseElement();
    builder.CloseElement();

    // Total
    builder.OpenElement(seq++, "p");
    builder.AddAttribute(seq++, "class", "total");
    builder.AddContent(seq++, $"Total: ${order.Total:F2}");
    builder.CloseElement();

    builder.CloseElement();
};
```

### Nested Model Binding

```csharp
public class BlogPost
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Author Author { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}

public class Author
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class Comment
{
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Posted { get; set; }
}

// Author card template
RenderFragment<Author> authorCard = author => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "author-card");

    builder.OpenElement(seq++, "h4");
    builder.AddContent(seq++, author.Name);
    builder.CloseElement();

    if (!string.IsNullOrEmpty(author.Email))
    {
        builder.OpenElement(seq++, "p");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", $"mailto:{author.Email}");
        builder.AddContent(seq++, author.Email);
        builder.CloseElement();
        builder.CloseElement();
    }

    if (!string.IsNullOrEmpty(author.Bio))
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, author.Bio);
        builder.CloseElement();
    }

    builder.CloseElement();
};

// Comment template
RenderFragment<Comment> commentTemplate = comment => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "comment");

    builder.OpenElement(seq++, "p");
    builder.AddAttribute(seq++, "class", "comment-author");
    builder.AddContent(seq++, comment.Author);
    builder.AddContent(seq++, " - ");
    builder.AddContent(seq++, comment.Posted.ToString("MMM dd, yyyy"));
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, comment.Text);
    builder.CloseElement();

    builder.CloseElement();
};

// Blog post template using nested templates
RenderFragment<BlogPost> blogPostTemplate = post => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "article");

    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, post.Title);
    builder.CloseElement();

    // Author section using nested template
    builder.AddContent(seq++, authorCard(post.Author));

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "content");
    builder.AddContent(seq++, post.Content);
    builder.CloseElement();

    // Comments section
    if (post.Comments.Any())
    {
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "comments");

        builder.OpenElement(seq++, "h3");
        builder.AddContent(seq++, $"Comments ({post.Comments.Count})");
        builder.CloseElement();

        foreach (var comment in post.Comments)
        {
            builder.AddContent(seq++, commentTemplate(comment));
        }

        builder.CloseElement();
    }

    builder.CloseElement();
};
```

## Advanced Patterns

### Conditional Rendering

```csharp
RenderFragment<User> userProfile = user => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");

    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, user.Name);
    builder.CloseElement();

    // Conditional rendering based on user state
    if (user.IsVerified)
    {
        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "badge badge-verified");
        builder.AddContent(seq++, "✓ Verified");
        builder.CloseElement();
    }
    else
    {
        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "badge badge-unverified");
        builder.AddContent(seq++, "Not Verified");
        builder.CloseElement();
    }

    // Conditional rendering with null checks
    if (!string.IsNullOrEmpty(user.Bio))
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, user.Bio);
        builder.CloseElement();
    }

    builder.CloseElement();
};
```

### List Rendering

```csharp
RenderFragment<List<Product>> productList = products => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "product-grid");

    if (products.Count == 0)
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "No products found.");
        builder.CloseElement();
    }
    else
    {
        foreach (var product in products)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "product-card");

            builder.OpenElement(seq++, "h3");
            builder.AddContent(seq++, product.Name);
            builder.CloseElement();

            builder.OpenElement(seq++, "p");
            builder.AddContent(seq++, $"${product.Price:F2}");
            builder.CloseElement();

            builder.CloseElement();
        }
    }

    builder.CloseElement();
};
```

### Template Composition

```csharp
// Base layout template
RenderFragment<(string Title, RenderFragment Content)> layout =
    data => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "html");
    builder.OpenElement(seq++, "head");
    builder.OpenElement(seq++, "title");
    builder.AddContent(seq++, data.Title);
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "body");
    builder.OpenElement(seq++, "header");
    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, data.Title);
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "main");
    builder.AddContent(seq++, data.Content);
    builder.CloseElement();

    builder.OpenElement(seq++, "footer");
    builder.AddContent(seq++, "© 2024");
    builder.CloseElement();
    builder.CloseElement();

    builder.CloseElement();
};

// Usage
app.MapGet("/page", () =>
{
    var content = CreateContent("Page content here");
    return RazorResults.Razor(layout, ("My Page", content));
});
```

### Rendering to String

```csharp
app.MapGet("/email/{userId}", async (int userId, IServiceProvider services) =>
{
    var user = await GetUserAsync(userId);

    // Render template to string for email
    var emailHtml = await userEmailTemplate.RenderAsync(user, services);

    // Send email
    await emailService.SendAsync(user.Email, "Welcome", emailHtml);

    return Results.Ok("Email sent");
});
```

## Configuration

### Service Registration Options

```csharp
var builder = WebApplication.CreateBuilder(args);

// Basic registration
builder.Services.AddRazorHelpers();

// You can also add custom services that your templates might need
builder.Services.AddSingleton<IMyCustomService, MyCustomService>();
```

### Custom HTTP Status Codes

```csharp
// Return 201 Created
app.MapPost("/resource", () =>
{
    return RazorResults.Razor(template, 201);
});

// Return 404 Not Found
app.MapGet("/missing", () =>
{
    return RazorResults.Razor(notFoundTemplate, 404);
});
```

### Custom Content Types

```csharp
// Return as XML
app.MapGet("/xml", () =>
{
    return RazorResults.Razor(xmlTemplate, null, "application/xml");
});

// Return as plain text
app.MapGet("/text", () =>
{
    return RazorResults.Razor(textTemplate, null, "text/plain");
});
```

## Next Steps

- See [API Reference](API.md) for complete API documentation
- See [Patterns](PATTERNS.md) for design patterns and best practices
- See [Examples](EXAMPLES.md) for real-world usage examples
- See [Troubleshooting](TROUBLESHOOTING.md) for common issues and solutions
