# RazorHelpers Patterns and Best Practices

Comprehensive guide to design patterns and best practices when using RazorHelpers.

## Table of Contents

- [Template Organization](#template-organization)
- [Model Binding Patterns](#model-binding-patterns)
- [Composition Patterns](#composition-patterns)
- [Performance Optimization](#performance-optimization)
- [Testing Patterns](#testing-patterns)
- [Security Best Practices](#security-best-practices)
- [Common Anti-Patterns](#common-anti-patterns)

## Template Organization

### Pattern: Template Repository

**Problem**: Templates scattered throughout the codebase make them hard to find and maintain.

**Solution**: Centralize templates in a static class or service.

```csharp
public static class Templates
{
    // User-related templates
    public static class Users
    {
        public static RenderFragment<User> Card => user => builder =>
        {
            var seq = 0;
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "user-card");
            builder.OpenElement(seq++, "h3");
            builder.AddContent(seq++, user.Name);
            builder.CloseElement();
            builder.CloseElement();
        };

        public static RenderFragment<User> DetailView => user => builder =>
        {
            // Detailed user view implementation
        };

        public static RenderFragment<IEnumerable<User>> List => users => builder =>
        {
            // User list implementation
        };
    }

    // Product-related templates
    public static class Products
    {
        public static RenderFragment<Product> Card => product => builder =>
        {
            // Product card implementation
        };

        public static RenderFragment<Product> DetailView => product => builder =>
        {
            // Product detail implementation
        };
    }

    // Layout templates
    public static class Layouts
    {
        public static RenderFragment<(string Title, RenderFragment Content)> Main =>
            data => builder =>
        {
            // Main layout implementation
        };
    }
}

// Usage
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(Templates.Users.Card, user);
});
```

**Benefits**:
- Easy to find templates
- Organized by domain
- Encourages reuse
- Easier to test

### Pattern: Template Factory

**Problem**: Complex templates with varying configurations.

**Solution**: Use factory methods to create configured templates.

```csharp
public static class TemplateFactory
{
    public static RenderFragment<Product> CreateProductCard(ProductCardOptions options)
    {
        return product => builder =>
        {
            var seq = 0;
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", $"product-card {options.CssClass}");

            if (options.ShowImage && !string.IsNullOrEmpty(product.ImageUrl))
            {
                builder.OpenElement(seq++, "img");
                builder.AddAttribute(seq++, "src", product.ImageUrl);
                builder.CloseElement();
            }

            builder.OpenElement(seq++, "h3");
            builder.AddContent(seq++, product.Name);
            builder.CloseElement();

            if (options.ShowPrice)
            {
                builder.OpenElement(seq++, "p");
                builder.AddContent(seq++, $"${product.Price:F2}");
                builder.CloseElement();
            }

            if (options.ShowDescription)
            {
                builder.OpenElement(seq++, "p");
                builder.AddContent(seq++, product.Description);
                builder.CloseElement();
            }

            builder.CloseElement();
        };
    }
}

public class ProductCardOptions
{
    public bool ShowImage { get; set; } = true;
    public bool ShowPrice { get; set; } = true;
    public bool ShowDescription { get; set; } = false;
    public string CssClass { get; set; } = string.Empty;
}

// Usage
var cardTemplate = TemplateFactory.CreateProductCard(new ProductCardOptions
{
    ShowDescription = true,
    CssClass = "featured"
});

app.MapGet("/products/featured", () =>
{
    var products = GetFeaturedProducts();
    return RazorResults.Razor(cardTemplate, products);
});
```

## Model Binding Patterns

### Pattern: View Model Transformation

**Problem**: Domain models don't always match UI requirements.

**Solution**: Create view models specifically for rendering.

```csharp
// Domain model
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<Order> Orders { get; set; } = new();
}

// View model
public class UserCardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }

    public static UserCardViewModel FromUser(User user)
    {
        return new UserCardViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Age = DateTime.Now.Year - user.BirthDate.Year,
            Email = user.Email,
            TotalOrders = user.Orders.Count,
            TotalSpent = user.Orders.Sum(o => o.Total)
        };
    }
}

// Template uses view model
RenderFragment<UserCardViewModel> userCardTemplate = vm => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "user-card");

    builder.OpenElement(seq++, "h3");
    builder.AddContent(seq++, vm.FullName);
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Age: {vm.Age}");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Orders: {vm.TotalOrders} (${vm.TotalSpent:F2})");
    builder.CloseElement();

    builder.CloseElement();
};

// Usage
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    var viewModel = UserCardViewModel.FromUser(user);
    return RazorResults.Razor(userCardTemplate, viewModel);
});
```

**Benefits**:
- Clean separation of concerns
- Templates are simpler
- Easier to test
- Reusable transformations

### Pattern: Projection with LINQ

**Problem**: Loading too much data from the database.

**Solution**: Project directly to view models in queries.

```csharp
public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
}

// Efficient database query with projection
app.MapGet("/products", async (AppDbContext db) =>
{
    var products = await db.Products
        .Select(p => new ProductListItemViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            ReviewCount = p.Reviews.Count,
            AverageRating = p.Reviews.Average(r => r.Rating)
        })
        .ToListAsync();

    return RazorResults.Razor(productListTemplate, products);
});
```

### Pattern: Nullable Model Handling

**Problem**: Handling optional or nullable model properties safely.

**Solution**: Use null-conditional operators and provide defaults.

```csharp
RenderFragment<User?> userTemplate = user => builder =>
{
    var seq = 0;

    if (user == null)
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "User not found");
        builder.CloseElement();
        return;
    }

    builder.OpenElement(seq++, "div");

    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, user.Name ?? "Unknown User");
    builder.CloseElement();

    // Safe null handling for nested properties
    if (user.Address != null)
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, $"{user.Address.City}, {user.Address.State}");
        builder.CloseElement();
    }

    // Null-coalescing with defaults
    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Email: {user.Email ?? "Not provided"}");
    builder.CloseElement();

    builder.CloseElement();
};
```

## Composition Patterns

### Pattern: Component-Based Composition

**Problem**: Large templates are hard to maintain and test.

**Solution**: Break templates into smaller, composable pieces.

```csharp
public static class Components
{
    // Atomic component: Avatar
    public static RenderFragment<string> Avatar => imageUrl => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "img");
        builder.AddAttribute(seq++, "src", imageUrl);
        builder.AddAttribute(seq++, "class", "avatar");
        builder.CloseElement();
    };

    // Atomic component: Badge
    public static RenderFragment<(string Text, string Color)> Badge =>
        data => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", $"badge badge-{data.Color}");
        builder.AddContent(seq++, data.Text);
        builder.CloseElement();
    };

    // Molecular component: User Header (composed of atoms)
    public static RenderFragment<User> UserHeader => user => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "user-header");

        // Use Avatar component
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            builder.AddContent(seq++, Avatar(user.AvatarUrl));
        }

        builder.OpenElement(seq++, "div");
        builder.AddContent(seq++, user.Name);

        // Use Badge component
        if (user.IsVerified)
        {
            builder.AddContent(seq++, Badge(("Verified", "success")));
        }

        builder.CloseElement();
        builder.CloseElement();
    };

    // Organism: User Card (composed of molecules and atoms)
    public static RenderFragment<User> UserCard => user => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "user-card");

        // Use UserHeader component
        builder.AddContent(seq++, UserHeader(user));

        // Additional user card content
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "user-body");
        builder.AddContent(seq++, user.Bio ?? "No bio available");
        builder.CloseElement();

        builder.CloseElement();
    };
}
```

**Benefits**:
- Highly reusable components
- Easier to test individual pieces
- Clear composition hierarchy
- Follows Atomic Design principles

### Pattern: Layout Wrapper

**Problem**: Repetitive page structure across routes.

**Solution**: Create reusable layout templates.

```csharp
public static class Layouts
{
    public static RenderFragment<PageModel> StandardPage => model => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "html");

        // Head
        builder.OpenElement(seq++, "head");
        builder.OpenElement(seq++, "title");
        builder.AddContent(seq++, model.Title);
        builder.CloseElement();
        builder.OpenElement(seq++, "link");
        builder.AddAttribute(seq++, "rel", "stylesheet");
        builder.AddAttribute(seq++, "href", "/styles.css");
        builder.CloseElement();
        builder.CloseElement();

        // Body
        builder.OpenElement(seq++, "body");

        // Header
        builder.OpenElement(seq++, "header");
        builder.AddContent(seq++, Navigation);
        builder.CloseElement();

        // Main content
        builder.OpenElement(seq++, "main");
        if (!string.IsNullOrEmpty(model.Heading))
        {
            builder.OpenElement(seq++, "h1");
            builder.AddContent(seq++, model.Heading);
            builder.CloseElement();
        }
        builder.AddContent(seq++, model.Content);
        builder.CloseElement();

        // Footer
        builder.OpenElement(seq++, "footer");
        builder.AddContent(seq++, Footer);
        builder.CloseElement();

        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment Navigation => builder =>
    {
        // Navigation implementation
    };

    private static RenderFragment Footer => builder =>
    {
        // Footer implementation
    };
}

public class PageModel
{
    public string Title { get; set; } = string.Empty;
    public string? Heading { get; set; }
    public RenderFragment Content { get; set; } = _ => { };
}

// Usage
app.MapGet("/about", () =>
{
    var page = new PageModel
    {
        Title = "About Us",
        Heading = "About Our Company",
        Content = builder =>
        {
            builder.AddContent(0, "Company information here...");
        }
    };

    return RazorResults.Razor(Layouts.StandardPage, page);
});
```

### Pattern: Higher-Order Templates

**Problem**: Need to apply common transformations or wrappers.

**Solution**: Create templates that accept and wrap other templates.

```csharp
public static class Wrappers
{
    // Wrapper that adds error boundaries
    public static RenderFragment<TModel> WithErrorBoundary<TModel>(
        RenderFragment<TModel> innerTemplate,
        string errorMessage = "An error occurred")
    {
        return model => builder =>
        {
            try
            {
                builder.AddContent(0, innerTemplate(model));
            }
            catch (Exception ex)
            {
                var seq = 0;
                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "error-boundary");
                builder.OpenElement(seq++, "p");
                builder.AddContent(seq++, errorMessage);
                builder.CloseElement();
                builder.OpenElement(seq++, "small");
                builder.AddContent(seq++, ex.Message);
                builder.CloseElement();
                builder.CloseElement();
            }
        };
    }

    // Wrapper that adds loading state
    public static RenderFragment<TModel?> WithLoading<TModel>(
        RenderFragment<TModel> innerTemplate)
        where TModel : class
    {
        return model => builder =>
        {
            if (model == null)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "loading");
                builder.AddContent(2, "Loading...");
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(0, innerTemplate(model));
            }
        };
    }

    // Wrapper that adds caching information
    public static RenderFragment<(TModel Data, DateTime CachedAt)> WithCacheInfo<TModel>(
        RenderFragment<TModel> innerTemplate)
    {
        return data => builder =>
        {
            builder.AddContent(0, innerTemplate(data.Data));

            var seq = 1;
            builder.OpenElement(seq++, "small");
            builder.AddAttribute(seq++, "class", "cache-info");
            builder.AddContent(seq++, $"Cached at: {data.CachedAt:HH:mm:ss}");
            builder.CloseElement();
        };
    }
}

// Usage
var safeUserTemplate = Wrappers.WithErrorBoundary(userTemplate);
var loadableTemplate = Wrappers.WithLoading(productTemplate);

app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(safeUserTemplate, user);
});
```

## Performance Optimization

### Pattern: Template Precompilation

**Problem**: Template creation overhead on each request.

**Solution**: Define templates as static readonly fields.

```csharp
public static class OptimizedTemplates
{
    // Good: Compiled once, reused many times
    private static readonly RenderFragment<User> _userCard = user => builder =>
    {
        // Template implementation
    };

    public static RenderFragment<User> UserCard => _userCard;
}

// In your endpoints
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(OptimizedTemplates.UserCard, user);
});
```

### Pattern: Lazy Template Loading

**Problem**: All templates loaded at startup even if not used.

**Solution**: Use Lazy<T> for templates that are rarely used.

```csharp
public static class LazyTemplates
{
    private static readonly Lazy<RenderFragment<Report>> _complexReport =
        new Lazy<RenderFragment<Report>>(() => report => builder =>
        {
            // Complex template that's rarely used
        });

    public static RenderFragment<Report> ComplexReport => _complexReport.Value;
}
```

### Pattern: String Building Optimization

**Problem**: Building large strings in templates is slow.

**Solution**: Use StringBuilder for complex string operations before rendering.

```csharp
RenderFragment<List<Product>> optimizedList = products => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "ul");

    // Pre-process data
    var sortedProducts = products.OrderBy(p => p.Name).ToList();

    foreach (var product in sortedProducts)
    {
        builder.OpenElement(seq++, "li");
        builder.AddContent(seq++, product.Name);
        builder.CloseElement();
    }

    builder.CloseElement();
};
```

### Pattern: Conditional Rendering Optimization

**Problem**: Unnecessary work done for elements that won't be rendered.

**Solution**: Check conditions early and return if not rendering.

```csharp
RenderFragment<User> conditionalTemplate = user => builder =>
{
    // Early return if user doesn't meet criteria
    if (!user.IsActive)
    {
        return;
    }

    var seq = 0;
    // Only do expensive work if user is active
    builder.OpenElement(seq++, "div");
    // ... complex rendering
    builder.CloseElement();
};
```

## Testing Patterns

### Pattern: Template Unit Testing

**Problem**: Hard to verify template output without integration tests.

**Solution**: Render templates to strings and assert on the output.

```csharp
[Fact]
public async Task UserCard_RendersCorrectly()
{
    // Arrange
    var services = TestServiceProvider.Create();
    var user = new User { Name = "Test User", Age = 30 };

    // Act
    var html = await Templates.UserCard.RenderAsync(user, services);

    // Assert
    Assert.Contains("Test User", html);
    Assert.Contains("30", html);
    Assert.Contains("user-card", html);
}
```

### Pattern: Snapshot Testing

**Problem**: Regression testing for complex templates.

**Solution**: Store expected output and compare.

```csharp
[Fact]
public async Task ComplexTemplate_MatchesSnapshot()
{
    // Arrange
    var services = TestServiceProvider.Create();
    var data = GetTestData();

    // Act
    var html = await complexTemplate.RenderAsync(data, services);

    // Assert
    var normalizedHtml = NormalizeHtml(html);
    var expectedHtml = File.ReadAllText("snapshots/complex-template.html");
    Assert.Equal(expectedHtml, normalizedHtml);
}
```

## Security Best Practices

### Pattern: HTML Encoding

**Problem**: XSS vulnerabilities from unencoded user input.

**Solution**: Always encode user-provided content.

```csharp
using System.Net;

RenderFragment<Comment> safeCommentTemplate = comment => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");

    // SAFE: AddContent automatically encodes
    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, comment.Text);  // Automatically encoded
    builder.CloseElement();

    // DANGEROUS: Don't use AddMarkupContent for user input
    // builder.AddMarkupContent(seq++, comment.Text);  // NOT SAFE!

    // If you must include HTML, sanitize first
    if (!string.IsNullOrEmpty(comment.SafeHtml))
    {
        var sanitized = HtmlSanitizer.Sanitize(comment.SafeHtml);
        builder.AddMarkupContent(seq++, sanitized);
    }

    builder.CloseElement();
};
```

### Pattern: URL Sanitization

**Problem**: Open redirect vulnerabilities.

**Solution**: Validate and sanitize URLs.

```csharp
RenderFragment<Link> safeLinkTemplate = link => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "a");

    // Validate URL before using
    if (Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
    {
        builder.AddAttribute(seq++, "href", uri.ToString());
    }
    else
    {
        // Use safe default or show error
        builder.AddAttribute(seq++, "href", "#");
    }

    builder.AddContent(seq++, link.Text);
    builder.CloseElement();
};
```

## Common Anti-Patterns

### Anti-Pattern: Template in Endpoint

**Problem**: Templates defined inline in endpoints.

```csharp
// ❌ BAD: Template logic mixed with routing logic
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    RenderFragment<User> template = u => builder =>
    {
        // Inline template definition
    };
    return RazorResults.Razor(template, user);
});
```

**Solution**: Extract to separate template class.

```csharp
// ✅ GOOD: Separated concerns
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(Templates.UserCard, user);
});
```

### Anti-Pattern: Mixing Business Logic with Rendering

**Problem**: Business logic inside templates.

```csharp
// ❌ BAD: Business logic in template
RenderFragment<Order> template = order => builder =>
{
    // Don't do calculations in template
    var tax = order.Subtotal * 0.08m;
    var total = order.Subtotal + tax;

    builder.AddContent(0, $"Total: ${total}");
};
```

**Solution**: Prepare data before rendering.

```csharp
// ✅ GOOD: Business logic in model or service
public class OrderViewModel
{
    public decimal Subtotal { get; set; }
    public decimal Tax => Subtotal * 0.08m;
    public decimal Total => Subtotal + Tax;
}

RenderFragment<OrderViewModel> template = vm => builder =>
{
    builder.AddContent(0, $"Total: ${vm.Total}");
};
```

### Anti-Pattern: God Templates

**Problem**: One template doing everything.

```csharp
// ❌ BAD: Monolithic template
RenderFragment<DashboardData> hugeDashboard = data => builder =>
{
    // 500+ lines of rendering logic
    // Multiple responsibilities
    // Hard to test and maintain
};
```

**Solution**: Break into smaller, focused templates.

```csharp
// ✅ GOOD: Composed templates
RenderFragment<DashboardData> dashboard = data => builder =>
{
    builder.AddContent(0, DashboardHeader(data.Header));
    builder.AddContent(1, DashboardMetrics(data.Metrics));
    builder.AddContent(2, DashboardCharts(data.Charts));
    builder.AddContent(3, DashboardActivity(data.Activity));
};
```

---

## See Also

- [Usage Guide](USAGE.md)
- [API Reference](API.md)
- [Examples](EXAMPLES.md)
- [Troubleshooting](TROUBLESHOOTING.md)
