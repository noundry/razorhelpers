# RazorHelpers API Reference

Complete API reference for all public types and methods in RazorHelpers.

## Table of Contents

- [RazorResults](#razorresults)
- [RenderFragmentExtensions](#renderfragmentextensions)
- [ComponentHelper](#componenthelper)
- [ServiceCollectionExtensions](#servicecollectionextensions)
- [HtmlResultsExtensions](#htmlresultsextensions)

## RazorResults

Static helper class for creating Razor-based IResult responses.

**Namespace**: `RazorHelpers`

### Methods

#### Razor(RenderFragment, int?, string?)

Creates a RazorComponentResult from a RenderFragment.

```csharp
public static RazorComponentResult Razor(
    RenderFragment fragment,
    int? statusCode = null,
    string? contentType = null)
```

**Parameters:**
- `fragment` (RenderFragment): The RenderFragment to render. **Required.**
- `statusCode` (int?): Optional HTTP status code. Default: null (200 OK)
- `contentType` (string?): Optional content type. Default: null ("text/html; charset=utf-8")

**Returns:** `RazorComponentResult` - Can be returned from minimal API endpoints.

**Exceptions:**
- `ArgumentNullException`: Thrown when fragment is null.

**Example:**
```csharp
app.MapGet("/", () => RazorResults.Razor(myFragment));

// With custom status code
app.MapPost("/created", () => RazorResults.Razor(template, 201));

// With custom content type
app.MapGet("/xml", () => RazorResults.Razor(template, null, "application/xml"));
```

#### Razor<TModel>(RenderFragment<TModel>, TModel, int?, string?)

Creates a RazorComponentResult from a RenderFragment with a strongly-typed model.

```csharp
public static RazorComponentResult Razor<TModel>(
    RenderFragment<TModel> fragment,
    TModel model,
    int? statusCode = null,
    string? contentType = null)
```

**Type Parameters:**
- `TModel`: The type of the model.

**Parameters:**
- `fragment` (RenderFragment<TModel>): The RenderFragment accepting a model parameter. **Required.**
- `model` (TModel): The model to pass to the RenderFragment. **Required.**
- `statusCode` (int?): Optional HTTP status code. Default: null (200 OK)
- `contentType` (string?): Optional content type. Default: null ("text/html; charset=utf-8")

**Returns:** `RazorComponentResult` - Can be returned from minimal API endpoints.

**Exceptions:**
- `ArgumentNullException`: Thrown when fragment or model is null.

**Example:**
```csharp
RenderFragment<User> userTemplate = user => builder => { /* ... */ };

app.MapGet("/user/{id}", (int id) =>
{
    var user = new User { Id = id, Name = "John" };
    return RazorResults.Razor(userTemplate, user);
});
```

---

## RenderFragmentExtensions

Extension methods for rendering RenderFragment instances to HTML strings.

**Namespace**: `RazorHelpers`

### Extension Methods

#### RenderAsync(this RenderFragment, IServiceProvider)

Renders a RenderFragment to an HTML string asynchronously.

```csharp
public static async Task<string> RenderAsync(
    this RenderFragment fragment,
    IServiceProvider services)
```

**Parameters:**
- `fragment` (RenderFragment): The RenderFragment to render. **Required.**
- `services` (IServiceProvider): The service provider containing required services (ILoggerFactory). **Required.**

**Returns:** `Task<string>` - A task containing the rendered HTML string.

**Exceptions:**
- `ArgumentNullException`: Thrown when fragment or services is null.

**Example:**
```csharp
app.MapGet("/html", async (IServiceProvider services) =>
{
    var html = await myFragment.RenderAsync(services);
    return Results.Content(html, "text/html");
});
```

#### RenderAsync<TModel>(this RenderFragment<TModel>, TModel, IServiceProvider)

Renders a RenderFragment with a strongly-typed model to an HTML string asynchronously.

```csharp
public static Task<string> RenderAsync<TModel>(
    this RenderFragment<TModel> fragment,
    TModel model,
    IServiceProvider services)
```

**Type Parameters:**
- `TModel`: The type of the model.

**Parameters:**
- `fragment` (RenderFragment<TModel>): The RenderFragment accepting a model parameter. **Required.**
- `model` (TModel): The model to pass to the RenderFragment. **Required.**
- `services` (IServiceProvider): The service provider containing required services. **Required.**

**Returns:** `Task<string>` - A task containing the rendered HTML string.

**Exceptions:**
- `ArgumentNullException`: Thrown when fragment, model, or services is null.

**Example:**
```csharp
app.MapGet("/email/{userId}", async (int userId, IServiceProvider services) =>
{
    var user = await GetUserAsync(userId);
    var emailHtml = await emailTemplate.RenderAsync(user, services);
    await SendEmailAsync(user.Email, emailHtml);
    return Results.Ok();
});
```

---

## ComponentHelper

Static helper class for rendering Razor component classes.

**Namespace**: `RazorHelpers`

### Methods

#### RenderComponentAsync<TComponent>(IServiceProvider, IReadOnlyDictionary<string, object?>?)

Renders a component of the specified type to an HTML string.

```csharp
public static async Task<string> RenderComponentAsync<TComponent>(
    IServiceProvider services,
    IReadOnlyDictionary<string, object?>? parameters = null)
    where TComponent : IComponent
```

**Type Parameters:**
- `TComponent`: The type of component to render. Must implement `IComponent`.

**Parameters:**
- `services` (IServiceProvider): The service provider containing required services. **Required.**
- `parameters` (IReadOnlyDictionary<string, object?>?): Optional parameters to pass to the component. Default: null

**Returns:** `Task<string>` - A task containing the rendered HTML string.

**Exceptions:**
- `ArgumentNullException`: Thrown when services is null.

**Example:**
```csharp
// Without parameters
app.MapGet("/component", async (IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<MyComponent>(services);
    return Results.Content(html, "text/html");
});

// With parameters
app.MapGet("/greeting/{name}", async (string name, IServiceProvider services) =>
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
```

#### RenderComponentAsync<TComponent, TValue>(IServiceProvider, string, TValue)

Renders a component with a single parameter to an HTML string.

```csharp
public static Task<string> RenderComponentAsync<TComponent, TValue>(
    IServiceProvider services,
    string parameterName,
    TValue parameterValue)
    where TComponent : IComponent
```

**Type Parameters:**
- `TComponent`: The type of component to render. Must implement `IComponent`.
- `TValue`: The type of the parameter value.

**Parameters:**
- `services` (IServiceProvider): The service provider containing required services. **Required.**
- `parameterName` (string): The name of the parameter. **Required.**
- `parameterValue` (TValue): The value of the parameter. **Required.**

**Returns:** `Task<string>` - A task containing the rendered HTML string.

**Exceptions:**
- `ArgumentNullException`: Thrown when services or parameterName is null.

**Example:**
```csharp
app.MapGet("/welcome/{name}", async (string name, IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<WelcomeComponent, string>(
        services, "Name", name);
    return Results.Content(html, "text/html");
});
```

---

## ServiceCollectionExtensions

Extension methods for configuring RazorHelpers services.

**Namespace**: `RazorHelpers`

### Extension Methods

#### AddRazorHelpers(this IServiceCollection)

Adds RazorHelpers services to the service collection, including Razor Components support.

```csharp
public static IServiceCollection AddRazorHelpers(
    this IServiceCollection services)
```

**Parameters:**
- `services` (IServiceCollection): The service collection to add services to. **Required.**

**Returns:** `IServiceCollection` - The service collection for chaining.

**Exceptions:**
- `ArgumentNullException`: Thrown when services is null.

**Example:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register RazorHelpers services
builder.Services.AddRazorHelpers();

// Chain with other service registrations
builder.Services
    .AddRazorHelpers()
    .AddSingleton<IMyService, MyService>();

var app = builder.Build();
```

---

## HtmlResultsExtensions

Extension methods for creating Razor-based IResult responses using the `IResultExtensions` interface.

**Namespace**: `RazorHelpers`

**Note**: These extensions work with `IResultExtensions`, which requires ASP.NET Core's `Results` class. For simpler usage, consider using the `RazorResults` static class instead.

### Extension Methods

#### Razor(this IResultExtensions, RenderFragment, int?, string?)

Creates a RazorComponentResult from a RenderFragment (extension method version).

```csharp
public static RazorComponentResult Razor(
    this IResultExtensions results,
    RenderFragment fragment,
    int? statusCode = null,
    string? contentType = null)
```

**Parameters:**
- `results` (IResultExtensions): The IResultExtensions instance. **Required.**
- `fragment` (RenderFragment): The RenderFragment to render. **Required.**
- `statusCode` (int?): Optional HTTP status code. Default: null
- `contentType` (string?): Optional content type. Default: null

**Returns:** `RazorComponentResult`

**Exceptions:**
- `ArgumentNullException`: Thrown when fragment is null.

**Example:**
```csharp
// Note: This requires proper setup of IResultExtensions
// For most use cases, RazorResults.Razor() is simpler
app.MapGet("/", () => Results.Razor(myFragment));
```

#### Razor<TModel>(this IResultExtensions, RenderFragment<TModel>, TModel, int?, string?)

Creates a RazorComponentResult from a RenderFragment with a model (extension method version).

```csharp
public static RazorComponentResult Razor<TModel>(
    this IResultExtensions results,
    RenderFragment<TModel> fragment,
    TModel model,
    int? statusCode = null,
    string? contentType = null)
```

**Type Parameters:**
- `TModel`: The type of the model.

**Parameters:**
- `results` (IResultExtensions): The IResultExtensions instance. **Required.**
- `fragment` (RenderFragment<TModel>): The RenderFragment accepting a model parameter. **Required.**
- `model` (TModel): The model to pass to the RenderFragment. **Required.**
- `statusCode` (int?): Optional HTTP status code. Default: null
- `contentType` (string?): Optional content type. Default: null

**Returns:** `RazorComponentResult`

**Exceptions:**
- `ArgumentNullException`: Thrown when fragment or model is null.

---

## Type Definitions

### RenderFragment

Delegate type representing a segment of UI content.

```csharp
public delegate void RenderFragment(RenderTreeBuilder builder);
```

**Usage:**
```csharp
RenderFragment myFragment = builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, "Content");
    builder.CloseElement();
};
```

### RenderFragment<T>

Generic delegate type representing a segment of UI content that accepts a parameter.

```csharp
public delegate RenderFragment RenderFragment<T>(T value);
```

**Usage:**
```csharp
RenderFragment<User> userTemplate = user => builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, user.Name);
    builder.CloseElement();
};
```

### RenderTreeBuilder

Class used to construct the UI tree. Key methods:

| Method | Description |
|--------|-------------|
| `OpenElement(int, string)` | Opens an HTML element |
| `CloseElement()` | Closes the current element |
| `AddContent(int, string)` | Adds text content |
| `AddAttribute(int, string, object)` | Adds an attribute to the current element |
| `AddMarkupContent(int, string)` | Adds raw HTML markup |

**Sequence Numbers:**
- Must be unique within each method scope
- Should be in ascending order
- Can use a counter variable: `var seq = 0;` then `seq++` for each call

---

## Common Patterns

### Creating a Template Repository

```csharp
public static class Templates
{
    public static RenderFragment<User> UserCard => user => builder =>
    {
        // Template implementation
    };

    public static RenderFragment<Product> ProductCard => product => builder =>
    {
        // Template implementation
    };

    // More templates...
}

// Usage
app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(Templates.UserCard, user);
});
```

### Dependency Injection in Templates

```csharp
// Register services
builder.Services.AddSingleton<IMyService, MyService>();

// Templates can't directly inject services, but can accept them as parameters
RenderFragment<(User User, IMyService Service)> template =
    data => builder =>
{
    var result = data.Service.Process(data.User);
    builder.AddContent(0, result);
};

// Use with injected service
app.MapGet("/user/{id}", (int id, IMyService service) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(template, (user, service));
});
```

### Async Data Loading

```csharp
app.MapGet("/dashboard", async (IServiceProvider services, DataService dataService) =>
{
    // Load data asynchronously
    var data = await dataService.GetDashboardDataAsync();

    // Render template with data
    var html = await dashboardTemplate.RenderAsync(data, services);
    return Results.Content(html, "text/html");
});
```

---

## Performance Considerations

### Template Caching

Templates are delegates and can be reused. Define them once and reuse:

```csharp
// Good: Define once, reuse many times
private static readonly RenderFragment<User> _userTemplate = user => builder => { /* ... */ };

app.MapGet("/user/{id}", (int id) =>
{
    var user = GetUser(id);
    return RazorResults.Razor(_userTemplate, user);
});

// Bad: Creating new template instance on each request
app.MapGet("/user/{id}", (int id) =>
{
    RenderFragment<User> template = user => builder => { /* ... */ };
    var user = GetUser(id);
    return RazorResults.Razor(template, user);
});
```

### Sequence Number Optimization

Use a local counter for better performance:

```csharp
// Good: Use local counter
RenderFragment template = builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddContent(seq++, "Content");
    builder.CloseElement();
};

// Acceptable but less performant: Literal numbers
RenderFragment template = builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, "Content");
    builder.CloseElement();
};
```

---

## Version Compatibility

- **RazorHelpers 1.0.0**: Requires .NET 9.0 or later
- **ASP.NET Core**: 9.0 or later

---

## See Also

- [Usage Guide](USAGE.md)
- [Patterns and Best Practices](PATTERNS.md)
- [Examples](EXAMPLES.md)
- [Troubleshooting](TROUBLESHOOTING.md)
