using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorHelpers;

var builder = WebApplication.CreateBuilder(args);

// Register RazorHelpers services
builder.Services.AddRazorHelpers();

var app = builder.Build();

// Example 1: Simple template using RenderTreeBuilder
app.MapGet("/", () => RazorResults.Razor(Templates.Home()));

// Example 2: Template with model - User card
app.MapGet("/user/{id:int}", (int id) =>
{
    var user = new User { Id = id, Name = $"User {id}", Email = $"user{id}@example.com", Age = 20 + id };
    return RazorResults.Razor(Templates.UserCard(user));
});

// Example 3: List rendering
app.MapGet("/list", () =>
{
    var users = new[]
    {
        new User { Id = 1, Name = "Alice", Email = "alice@example.com", Age = 25 },
        new User { Id = 2, Name = "Bob", Email = "bob@example.com", Age = 30 },
        new User { Id = 3, Name = "Charlie", Email = "charlie@example.com", Age = 35 }
    };
    return RazorResults.Razor(Templates.UserList(users));
});

// Example 4: Rendering to string
app.MapGet("/string", async (IServiceProvider services) =>
{
    var user = new User { Name = "String Example", Age = 42 };
    var html = await Templates.UserCard(user).RenderAsync(services);
    var response = $"<h2>Rendered as String:</h2><pre>{System.Net.WebUtility.HtmlEncode(html)}</pre><hr/><h2>Actual Rendering:</h2>{html}";
    return Results.Content(response, "text/html");
});

app.Run();

// Model classes
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Razor Templates using RenderTreeBuilder
public static class Templates
{
    public static RenderFragment Home() => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.OpenElement(seq++, "h1");
        builder.AddContent(seq++, "Welcome to RazorHelpers!");
        builder.CloseElement();
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "This is a simple inline Razor template.");
        builder.CloseElement();
        builder.OpenElement(seq++, "nav");
        builder.OpenElement(seq++, "ul");

        builder.OpenElement(seq++, "li");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", "/user/1");
        builder.AddContent(seq++, "User Example");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(seq++, "li");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", "/list");
        builder.AddContent(seq++, "List Example");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(seq++, "li");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", "/string");
        builder.AddContent(seq++, "String Rendering Example");
        builder.CloseElement();
        builder.CloseElement();

        builder.CloseElement(); // ul
        builder.CloseElement(); // nav
        builder.CloseElement(); // div
    };

    public static RenderFragment UserCard(User user) => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "card");
        builder.AddAttribute(seq++, "style", "border: 1px solid #ccc; padding: 20px; margin: 10px; border-radius: 8px; max-width: 400px;");

        builder.OpenElement(seq++, "h2");
        builder.AddAttribute(seq++, "style", "color: #333; margin-top: 0;");
        builder.AddContent(seq++, user.Name);
        builder.CloseElement();

        if (!string.IsNullOrEmpty(user.Email))
        {
            builder.OpenElement(seq++, "p");
            builder.AddAttribute(seq++, "style", "color: #666;");
            builder.OpenElement(seq++, "strong");
            builder.AddContent(seq++, "Email:");
            builder.CloseElement();
            builder.AddContent(seq++, " ");
            builder.OpenElement(seq++, "a");
            builder.AddAttribute(seq++, "href", $"mailto:{user.Email}");
            builder.AddContent(seq++, user.Email);
            builder.CloseElement();
            builder.CloseElement();
        }

        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "style", "color: #666;");
        builder.OpenElement(seq++, "strong");
        builder.AddContent(seq++, "Age:");
        builder.CloseElement();
        builder.AddContent(seq++, $" {user.Age} years old");
        builder.CloseElement();

        if (user.Id > 0)
        {
            builder.OpenElement(seq++, "p");
            builder.AddAttribute(seq++, "style", "color: #999; font-size: 0.9em;");
            builder.AddContent(seq++, $"User ID: {user.Id}");
            builder.CloseElement();
        }

        builder.CloseElement(); // div
    };

    public static RenderFragment UserList(User[] users) => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");

        builder.OpenElement(seq++, "h1");
        builder.AddContent(seq++, "User Directory");
        builder.CloseElement();

        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, $"Total users: {users.Length}");
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "style", "display: flex; flex-wrap: wrap;");

        foreach (var user in users)
        {
            builder.AddContent(seq++, UserCard(user));
        }

        builder.CloseElement(); // div

        builder.OpenElement(seq++, "p");
        builder.OpenElement(seq++, "a");
        builder.AddAttribute(seq++, "href", "/");
        builder.AddContent(seq++, "Back to Home");
        builder.CloseElement();
        builder.CloseElement();

        builder.CloseElement(); // div
    };
}
