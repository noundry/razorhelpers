using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorHelpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorHelpers();
var app = builder.Build();

// Example 1: ComponentHelper with no parameters
app.MapGet("/", async (IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<WelcomeComponent>(services);
    return Results.Content(html, "text/html");
});

// Example 2: ComponentHelper with parameters
app.MapGet("/greeting/{name}", async (string name, IServiceProvider services) =>
{
    var parameters = new Dictionary<string, object?>
    {
        ["Name"] = name,
        ["ShowTimestamp"] = true
    };
    var html = await ComponentHelper.RenderComponentAsync<GreetingComponent>(services, parameters);
    return Results.Content(html, "text/html");
});

// Example 3: ComponentHelper with single parameter convenience method
app.MapGet("/message/{text}", async (string text, IServiceProvider services) =>
{
    var html = await ComponentHelper.RenderComponentAsync<MessageComponent, string>(
        services, "Message", text);
    return Results.Content(html, "text/html");
});

// Example 4: HtmlBuilder - Simple fluent HTML building
app.MapGet("/html-builder", async (IServiceProvider services) =>
{
    var fragment = Html.Div()
        .Class("container")
        .Style("padding", "20px")
        .Style("font-family", "Arial")
        .Child(Html.H1("HtmlBuilder Demo").Style("color", "#333"))
        .Child(Html.P("Build HTML using a fluent, semantic API."))
        .Child(Html.Ul()
            .Child(Html.Li().Child(Html.A("/html-builder/cards", "Card Layout Demo")))
            .Child(Html.Li().Child(Html.A("/html-builder/table", "Table from Collection")))
            .Child(Html.Li().Child(Html.A("/html-builder/form", "Form with Select"))))
        .Render();

    var html = await fragment.RenderAsync(services);
    return Results.Content(html, "text/html");
});

// Example 5: HtmlBuilder - Card layout with nested elements
app.MapGet("/html-builder/cards", async (IServiceProvider services) =>
{
    var cards = new[]
    {
        ("RenderFragment", "Traditional Blazor rendering with RenderTreeBuilder API"),
        ("HtmlBuilder", "Fluent API for semantic HTML construction"),
        ("TableBuilder", "Easy table generation from collections"),
        ("SelectBuilder", "Dynamic dropdowns with optgroups")
    };

    var fragment = Html.Div()
        .Class("container")
        .Style("padding", "20px")
        .Style("font-family", "Arial")
        .Child(Html.H1("Card Layout Demo"))
        .Child(Html.Div()
            .Style("display", "grid")
            .Style("grid-template-columns", "repeat(2, 1fr)")
            .Style("gap", "16px")
            .Children(cards.Select(card => Html.Div()
                .Class("card")
                .Style("padding", "16px")
                .Style("border", "1px solid #ddd")
                .Style("border-radius", "8px")
                .Style("background", "#fafafa")
                .Child(Html.H3(card.Item1).Style("margin-top", "0").Style("color", "#2196F3"))
                .Child(Html.P(card.Item2).Style("color", "#666")))))
        .Child(Html.P().Child(Html.A("/html-builder", "Back to HtmlBuilder demos")))
        .Render();

    var html = await fragment.RenderAsync(services);
    return Results.Content(html, "text/html");
});

// Example 6: HtmlBuilder - Table from collection
app.MapGet("/html-builder/table", async (IServiceProvider services) =>
{
    var users = new[]
    {
        new User("John Doe", "john@example.com", "Admin", true),
        new User("Jane Smith", "jane@example.com", "Editor", true),
        new User("Bob Wilson", "bob@example.com", "Viewer", false),
        new User("Alice Brown", "alice@example.com", "Editor", true)
    };

    var fragment = Html.Div()
        .Class("container")
        .Style("padding", "20px")
        .Style("font-family", "Arial")
        .Child(Html.H1("Table from Collection"))
        .Child(Html.P("Using TableBuilder to generate tables from data:"))
        .Child(Html.Table(users)
            .Class("table")
            .Style("width", "100%")
            .Style("border-collapse", "collapse")
            .Caption("User Management")
            .Column("Name", u => Html.Strong(u.Name))
            .Column("Email", u => Html.A($"mailto:{u.Email}", u.Email))
            .Column("Role", u => u.Role)
            .Column("Status", u => Html.Span(u.IsActive ? "Active" : "Inactive")
                .Style("padding", "2px 8px")
                .Style("border-radius", "4px")
                .Style("background", u.IsActive ? "#4CAF50" : "#f44336")
                .Style("color", "white")))
        .Child(Html.StyleElement(@"
            .table th, .table td {
                padding: 12px;
                text-align: left;
                border-bottom: 1px solid #ddd;
            }
            .table th { background: #f5f5f5; }
            .table tr:hover { background: #f9f9f9; }
        "))
        .Child(Html.P().Child(Html.A("/html-builder", "Back to HtmlBuilder demos")))
        .Render();

    var html = await fragment.RenderAsync(services);
    return Results.Content(html, "text/html");
});

// Example 7: HtmlBuilder - Form with Select
app.MapGet("/html-builder/form", async (IServiceProvider services) =>
{
    var countries = new[]
    {
        new Country("us", "United States"),
        new Country("uk", "United Kingdom"),
        new Country("ca", "Canada"),
        new Country("de", "Germany"),
        new Country("fr", "France")
    };

    var roles = new[] { "Admin", "Editor", "Viewer" };

    var fragment = Html.Div()
        .Class("container")
        .Style("padding", "20px")
        .Style("font-family", "Arial")
        .Style("max-width", "500px")
        .Child(Html.H1("Form with Select Builders"))
        .Child(Html.Form("/submit", "POST")
            .Style("display", "flex")
            .Style("flex-direction", "column")
            .Style("gap", "16px")
            .Child(Html.Div()
                .Child(Html.Label("Name:", "name"))
                .Child(Html.Input("text", "name")
                    .Id("name")
                    .Class("form-control")
                    .Attr("placeholder", "Enter your name")
                    .Style("width", "100%")
                    .Style("padding", "8px")
                    .Style("margin-top", "4px")))
            .Child(Html.Div()
                .Child(Html.Label("Country:", "country"))
                .Child(Html.Select(countries, "country")
                    .Id("country")
                    .Class("form-control")
                    .Placeholder("Select a country...")
                    .Value(c => c.Code)
                    .Text(c => c.Name)
                    .Style("width", "100%")
                    .Style("padding", "8px")
                    .Style("margin-top", "4px")))
            .Child(Html.Div()
                .Child(Html.Label("Role:", "role"))
                .Child(Html.Select("role")
                    .Id("role")
                    .Class("form-control")
                    .Option("", "Select a role...")
                    .Option("admin", "Administrator")
                    .Option("editor", "Editor")
                    .Option("viewer", "Viewer")
                    .Style("width", "100%")
                    .Style("padding", "8px")
                    .Style("margin-top", "4px")))
            .Child(Html.Div()
                .Child(Html.Label("Vehicle:", "vehicle"))
                .Child(Html.Select("vehicle")
                    .Id("vehicle")
                    .OptGroup("Swedish Cars")
                        .Option("volvo", "Volvo")
                        .Option("saab", "Saab")
                    .EndGroup()
                    .OptGroup("German Cars")
                        .Option("mercedes", "Mercedes")
                        .Option("audi", "Audi")
                    .EndGroup()
                    .Style("width", "100%")
                    .Style("padding", "8px")
                    .Style("margin-top", "4px")))
            .Child(Html.Button("Submit", "submit")
                .Class("btn")
                .Style("padding", "10px 20px")
                .Style("background", "#2196F3")
                .Style("color", "white")
                .Style("border", "none")
                .Style("border-radius", "4px")
                .Style("cursor", "pointer")))
        .Child(Html.P().Child(Html.A("/html-builder", "Back to HtmlBuilder demos")))
        .Render();

    var html = await fragment.RenderAsync(services);
    return Results.Content(html, "text/html");
});

app.Run();

// Model definitions
record User(string Name, string Email, string Role, bool IsActive);
record Country(string Code, string Name);

// Component definitions
public class WelcomeComponent : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "style", "padding: 20px; font-family: Arial;");
        builder.OpenElement(seq++, "h1");
        builder.AddContent(seq++, "Advanced RazorHelpers Sample");
        builder.CloseElement();
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "This demonstrates ComponentHelper usage for rendering component classes.");
        builder.CloseElement();
        builder.OpenElement(seq++, "ul");
        builder.OpenElement(seq++, "li");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", "/greeting/John");
        builder.AddContent(seq++, "Greeting with parameter");
        builder.CloseElement();
        builder.CloseElement();
        builder.OpenElement(seq++, "li");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", "/message/Hello%20World");
        builder.AddContent(seq++, "Message example");
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
    }
}

public class GreetingComponent : ComponentBase
{
    [Parameter]
    public string Name { get; set; } = "Guest";

    [Parameter]
    public bool ShowTimestamp { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "style", "padding: 20px; border: 2px solid #4CAF50; border-radius: 8px; max-width: 500px;");
        builder.OpenElement(seq++, "h2");
        builder.AddAttribute(seq++, "style", "color: #4CAF50;");
        builder.AddContent(seq++, $"Hello, {Name}!");
        builder.CloseElement();
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, $"Welcome to RazorHelpers ComponentHelper demo.");
        builder.CloseElement();

        if (ShowTimestamp)
        {
            builder.OpenElement(seq++, "p");
            builder.AddAttribute(seq++, "style", "color: #666; font-size: 0.9em;");
            builder.AddContent(seq++, $"Rendered at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}

public class MessageComponent : ComponentBase
{
    [Parameter]
    public string Message { get; set; } = string.Empty;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "style", "padding: 15px; background-color: #f0f0f0; border-left: 4px solid #2196F3;");
        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "style", "margin: 0; font-size: 1.2em;");
        builder.AddContent(seq++, Message);
        builder.CloseElement();
        builder.CloseElement();
    }
}
