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

app.Run();

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
