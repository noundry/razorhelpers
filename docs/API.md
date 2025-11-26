# RazorHelpers API Reference

Complete API reference for all public types and methods in RazorHelpers.

## Table of Contents

- [RazorResults](#razorresults)
- [RenderFragmentExtensions](#renderfragmentextensions)
- [ComponentHelper](#componenthelper)
- [ServiceCollectionExtensions](#servicecollectionextensions)
- [HtmlResultsExtensions](#htmlresultsextensions)
- [Html (Static Class)](#html-static-class) ✨ New in v1.1.0
- [HtmlElement](#htmlelement) ✨ New in v1.1.0
- [HtmlVoidElement](#htmlvoidelement) ✨ New in v1.1.0
- [TableBuilder / TableBuilder\<T\>](#tablebuilder) ✨ New in v1.1.0
- [SelectBuilder / SelectBuilder\<T\>](#selectbuilder) ✨ New in v1.1.0

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

## Html (Static Class)

Static entry point for building HTML elements fluently. **New in v1.1.0**

**Namespace**: `RazorHelpers`

### Element Creation Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Div(content?)` | `HtmlElement` | Creates a `<div>` element |
| `Span(content?)` | `HtmlElement` | Creates a `<span>` element |
| `P(content?)` | `HtmlElement` | Creates a `<p>` element |
| `H1(content?)` - `H6(content?)` | `HtmlElement` | Creates heading elements |
| `A(href?, content?)` | `HtmlElement` | Creates an `<a>` element |
| `Img(src?, alt?)` | `HtmlVoidElement` | Creates an `<img>` element |
| `Button(content?, type?)` | `HtmlElement` | Creates a `<button>` element |
| `Form(action?, method?)` | `HtmlElement` | Creates a `<form>` element |
| `Input(type?, name?, value?)` | `HtmlVoidElement` | Creates an `<input>` element |
| `Textarea(name?, content?)` | `HtmlElement` | Creates a `<textarea>` element |
| `Label(content?, forId?)` | `HtmlElement` | Creates a `<label>` element |
| `Header(content?)` | `HtmlElement` | Creates a `<header>` element |
| `Footer(content?)` | `HtmlElement` | Creates a `<footer>` element |
| `Nav(content?)` | `HtmlElement` | Creates a `<nav>` element |
| `Main(content?)` | `HtmlElement` | Creates a `<main>` element |
| `Section(content?)` | `HtmlElement` | Creates a `<section>` element |
| `Article(content?)` | `HtmlElement` | Creates an `<article>` element |
| `Aside(content?)` | `HtmlElement` | Creates an `<aside>` element |
| `Ul()` | `HtmlElement` | Creates a `<ul>` element |
| `Ol()` | `HtmlElement` | Creates an `<ol>` element |
| `Li(content?)` | `HtmlElement` | Creates a `<li>` element |
| `Strong(content?)` | `HtmlElement` | Creates a `<strong>` element |
| `Em(content?)` | `HtmlElement` | Creates an `<em>` element |
| `B(content?)` | `HtmlElement` | Creates a `<b>` element (bold) |
| `I(content?)` | `HtmlElement` | Creates an `<i>` element (italic) |
| `U(content?)` | `HtmlElement` | Creates a `<u>` element (underline) |
| `S(content?)` | `HtmlElement` | Creates an `<s>` element (strikethrough) |
| `Code(content?)` | `HtmlElement` | Creates a `<code>` element |
| `Pre(content?)` | `HtmlElement` | Creates a `<pre>` element |
| `Kbd(content?)` | `HtmlElement` | Creates a `<kbd>` element (keyboard input) |
| `Samp(content?)` | `HtmlElement` | Creates a `<samp>` element (sample output) |
| `Var(content?)` | `HtmlElement` | Creates a `<var>` element (variable) |
| `Q(content?)` | `HtmlElement` | Creates a `<q>` element (inline quote) |
| `Address(content?)` | `HtmlElement` | Creates an `<address>` element |
| `Br()` | `HtmlVoidElement` | Creates a `<br>` element |
| `Hr()` | `HtmlVoidElement` | Creates an `<hr>` element |
| `Element(tagName, content?)` | `HtmlElement` | Creates a custom element |
| `VoidElement(tagName)` | `HtmlVoidElement` | Creates a custom void element |

### Collection Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Ul<T>(items, textSelector)` | `HtmlElement` | Creates `<ul>` with `<li>` for each item |
| `Ul<T>(items, elementSelector)` | `HtmlElement` | Creates `<ul>` with custom `<li>` content |
| `Ol<T>(items, textSelector)` | `HtmlElement` | Creates `<ol>` with `<li>` for each item |
| `Ol<T>(items, elementSelector)` | `HtmlElement` | Creates `<ol>` with custom `<li>` content |
| `Table()` | `TableBuilder` | Creates a table builder |
| `Table<T>(items)` | `TableBuilder<T>` | Creates a table builder from collection |
| `Select(name?)` | `SelectBuilder` | Creates a select builder |
| `Select<T>(items, name?)` | `SelectBuilder<T>` | Creates a select builder from collection |
| `Fragment(elements...)` | `RenderFragment` | Creates fragment from multiple elements |
| `Each<T>(items, builder)` | `RenderFragment` | Renders elements for each item |
| `Each<T>(items, builderWithIndex)` | `RenderFragment` | Renders elements with index |

**Example:**
```csharp
// Simple elements
var div = Html.Div("Hello").Class("greeting").Render();

// Lists from collections
var list = Html.Ul(items, x => x.Name);

// Tables from collections
var table = Html.Table(users)
    .Column("Name", u => u.Name)
    .Column("Email", u => u.Email)
    .Render();

// Selects from collections
var select = Html.Select(countries, "country")
    .Value(c => c.Code)
    .Text(c => c.Name)
    .Render();
```

---

## HtmlElement

Represents an HTML element that can be built fluently. **New in v1.1.0**

**Namespace**: `RazorHelpers`

### Constructors

```csharp
public HtmlElement(string tagName)
public HtmlElement(string tagName, string? content)
```

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Id(string)` | `HtmlElement` | Sets the id attribute |
| `Class(params string[])` | `HtmlElement` | Adds CSS classes |
| `ClassIf(string, bool)` | `HtmlElement` | Adds class conditionally |
| `Style(string, string)` | `HtmlElement` | Adds inline style |
| `Styles(IDictionary<string, string>)` | `HtmlElement` | Adds multiple styles |
| `Attr(string, object?)` | `HtmlElement` | Adds an attribute |
| `Attrs(IDictionary<string, object?>)` | `HtmlElement` | Adds multiple attributes |
| `Data(string, object?)` | `HtmlElement` | Adds data-* attribute |
| `Text(string?)` | `HtmlElement` | Sets text content |
| `Raw(string?)` | `HtmlElement` | Sets raw HTML content |
| `Content(RenderFragment)` | `HtmlElement` | Sets RenderFragment content |
| `Child(HtmlElement)` | `HtmlElement` | Adds child element |
| `Child(HtmlVoidElement)` | `HtmlElement` | Adds void element child |
| `Child(TableBuilder)` | `HtmlElement` | Adds table as child |
| `Child(SelectBuilder)` | `HtmlElement` | Adds select as child |
| `Children(params HtmlElement[])` | `HtmlElement` | Adds multiple children |
| `Children(IEnumerable<HtmlElement>)` | `HtmlElement` | Adds children from collection |
| `Render()` | `RenderFragment` | Converts to RenderFragment |

### Implicit Conversion

`HtmlElement` implicitly converts to `RenderFragment`:

```csharp
RenderFragment fragment = Html.Div("Hello");  // Implicit conversion
```

**Example:**
```csharp
var card = Html.Div()
    .Id("user-card")
    .Class("card", "shadow")
    .Style("padding", "20px")
    .Data("user-id", "123")
    .Child(Html.H1("John Doe"))
    .Child(Html.P("john@example.com"))
    .Render();
```

---

## HtmlVoidElement

Represents a void (self-closing) HTML element. **New in v1.1.0**

**Namespace**: `RazorHelpers`

Void elements include: `<br>`, `<hr>`, `<img>`, `<input>`, `<meta>`, `<link>`, `<source>`

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Id(string)` | `HtmlVoidElement` | Sets the id attribute |
| `Class(params string[])` | `HtmlVoidElement` | Adds CSS classes |
| `ClassIf(string, bool)` | `HtmlVoidElement` | Adds class conditionally |
| `Style(string, string)` | `HtmlVoidElement` | Adds inline style |
| `Styles(IDictionary<string, string>)` | `HtmlVoidElement` | Adds multiple styles |
| `Attr(string, object?)` | `HtmlVoidElement` | Adds an attribute |
| `Attrs(IDictionary<string, object?>)` | `HtmlVoidElement` | Adds multiple attributes |
| `Data(string, object?)` | `HtmlVoidElement` | Adds data-* attribute |
| `Render()` | `RenderFragment` | Converts to RenderFragment |

**Example:**
```csharp
var input = Html.Input("email", "email")
    .Id("email-input")
    .Class("form-control")
    .Attr("placeholder", "Enter email")
    .Attr("required", true)
    .Render();

var image = Html.Img("/logo.png", "Company Logo")
    .Class("logo")
    .Style("width", "200px")
    .Render();
```

---

## TableBuilder

Fluent builder for creating HTML tables. **New in v1.1.0**

**Namespace**: `RazorHelpers`

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Id(string)` | `TableBuilder` | Sets table id |
| `Class(params string[])` | `TableBuilder` | Adds CSS classes |
| `Style(string, string)` | `TableBuilder` | Adds inline style |
| `Attr(string, object?)` | `TableBuilder` | Adds attribute |
| `Caption(string)` | `TableBuilder` | Sets table caption |
| `Header(params string[])` | `TableBuilder` | Adds header cells |
| `Head(HtmlElement)` | `TableBuilder` | Sets custom thead |
| `Body(HtmlElement)` | `TableBuilder` | Sets custom tbody |
| `Foot(HtmlElement)` | `TableBuilder` | Sets custom tfoot |
| `Row(params string[])` | `TableBuilder` | Adds row with text cells |
| `Row(params HtmlElement[])` | `TableBuilder` | Adds row with element cells |
| `Render()` | `RenderFragment` | Converts to RenderFragment |

### TableBuilder\<T\> (Generic)

For creating tables from collections.

| Method | Returns | Description |
|--------|---------|-------------|
| `Header(params string[])` | `TableBuilder<T>` | Adds header cells |
| `Row(Func<T, string[]>)` | `TableBuilder<T>` | Defines row with text cells |
| `Row(Func<T, HtmlElement[]>)` | `TableBuilder<T>` | Defines row with element cells |
| `Row(Func<T, int, string[]>)` | `TableBuilder<T>` | Defines row with index |
| `Row(Func<T, int, HtmlElement[]>)` | `TableBuilder<T>` | Defines row with index |
| `Column(string, Func<T, string?>)` | `TableBuilder<T>` | Adds text column |
| `Column(string, Func<T, HtmlElement>)` | `TableBuilder<T>` | Adds element column |
| `RowClass(Func<T, string>)` | `TableBuilder<T>` | Sets row class selector |
| `RowAttrs(Func<T, Dictionary<...>>)` | `TableBuilder<T>` | Sets row attributes selector |
| `Caption(string)` | `TableBuilder<T>` | Sets table caption |
| `Foot(HtmlElement)` | `TableBuilder<T>` | Sets custom tfoot |
| `Render()` | `RenderFragment` | Converts to RenderFragment |

**Example:**
```csharp
// Manual table
var table = Html.Table()
    .Class("table", "table-striped")
    .Caption("Users")
    .Header("Name", "Email")
    .Row("John", "john@example.com")
    .Row("Jane", "jane@example.com")
    .Render();

// From collection
var table = Html.Table(users)
    .Class("table")
    .Column("Name", u => Html.Strong(u.Name))
    .Column("Email", u => Html.A($"mailto:{u.Email}", u.Email))
    .Column("Status", u => u.IsActive ? "Active" : "Inactive")
    .RowClass(u => u.IsActive ? "active" : "")
    .Render();
```

---

## SelectBuilder

Fluent builder for creating HTML select elements. **New in v1.1.0**

**Namespace**: `RazorHelpers`

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Id(string)` | `SelectBuilder` | Sets select id |
| `Name(string)` | `SelectBuilder` | Sets name attribute |
| `Class(params string[])` | `SelectBuilder` | Adds CSS classes |
| `Style(string, string)` | `SelectBuilder` | Adds inline style |
| `Attr(string, object?)` | `SelectBuilder` | Adds attribute |
| `Required(bool)` | `SelectBuilder` | Sets required attribute |
| `Disabled(bool)` | `SelectBuilder` | Sets disabled attribute |
| `Multiple(bool)` | `SelectBuilder` | Enables multiple selection |
| `Size(int)` | `SelectBuilder` | Sets visible option count |
| `Option(value, text, selected?, disabled?)` | `SelectBuilder` | Adds option |
| `OptGroup(label, disabled?)` | `SelectBuilder` | Starts optgroup |
| `EndGroup()` | `SelectBuilder` | Ends current optgroup |
| `Render()` | `RenderFragment` | Converts to RenderFragment |

### SelectBuilder\<T\> (Generic)

For creating selects from collections.

| Method | Returns | Description |
|--------|---------|-------------|
| `Placeholder(string)` | `SelectBuilder<T>` | Adds placeholder option |
| `Value(Func<T, string?>)` | `SelectBuilder<T>` | Sets value selector |
| `Text(Func<T, string>)` | `SelectBuilder<T>` | Sets text selector |
| `Selected(Func<T, bool>)` | `SelectBuilder<T>` | Sets selected predicate |
| `SelectedValue(string?)` | `SelectBuilder<T>` | Sets selected value |
| `DisabledOption(Func<T, bool>)` | `SelectBuilder<T>` | Sets disabled predicate |
| `GroupBy(Func<T, string>)` | `SelectBuilder<T>` | Groups into optgroups |
| `Render()` | `RenderFragment` | Converts to RenderFragment |

**Example:**
```csharp
// Manual select with optgroups
var select = Html.Select("car")
    .Class("form-select")
    .OptGroup("Swedish Cars")
        .Option("volvo", "Volvo")
        .Option("saab", "Saab")
    .EndGroup()
    .OptGroup("German Cars")
        .Option("mercedes", "Mercedes")
    .EndGroup()
    .Render();

// From collection with grouping
var select = Html.Select(cars, "car")
    .Placeholder("Select a car...")
    .Value(c => c.Code)
    .Text(c => c.Name)
    .GroupBy(c => c.Country)
    .SelectedValue("volvo")
    .Render();
```

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

- **RazorHelpers 1.1.0**: Adds HtmlBuilder, TableBuilder, SelectBuilder (requires .NET 9.0)
- **RazorHelpers 1.0.0**: Initial release with RenderFragment support (requires .NET 9.0)
- **ASP.NET Core**: 9.0 or later

---

## See Also

- [Usage Guide](USAGE.md)
- [Patterns and Best Practices](PATTERNS.md)
- [Examples](EXAMPLES.md)
- [Troubleshooting](TROUBLESHOOTING.md)
