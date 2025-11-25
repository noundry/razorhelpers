namespace RazorHelpers.Tests;

/// <summary>
/// Comprehensive validation tests for all HtmlBuilder collection scenarios.
/// These tests validate every example from the documentation actually works.
/// </summary>
public class HtmlBuilderValidationTests
{
    private readonly IServiceProvider _services;

    public HtmlBuilderValidationTests()
    {
        _services = TestServiceProvider.Create();
    }

    #region Table Collection Tests

    [Fact]
    public async Task Table_RowSelectorReturningStrings_Works()
    {
        // Arrange
        var users = new[]
        {
            new User("John", "john@example.com", true),
            new User("Jane", "jane@example.com", false)
        };

        // Act
        var table = Html.Table(users)
            .Header("Name", "Email", "Active")
            .Row(u => [u.Name, u.Email, u.IsActive ? "Yes" : "No"])
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("<table>", result);
        Assert.Contains("<th>Name</th>", result);
        Assert.Contains("<th>Email</th>", result);
        Assert.Contains("<th>Active</th>", result);
        Assert.Contains("<td>John</td>", result);
        Assert.Contains("<td>john@example.com</td>", result);
        Assert.Contains("<td>Yes</td>", result);
        Assert.Contains("<td>Jane</td>", result);
        Assert.Contains("<td>jane@example.com</td>", result);
        Assert.Contains("<td>No</td>", result);
    }

    [Fact]
    public async Task Table_RowSelectorReturningHtmlElements_Works()
    {
        // Arrange
        var users = new[]
        {
            new User("John", "john@example.com", true),
            new User("Jane", "jane@example.com", false)
        };

        // Act
        var table = Html.Table(users)
            .Header("Name", "Email", "Status")
            .Row(u => [
                Html.Strong(u.Name),
                Html.A($"mailto:{u.Email}", u.Email),
                Html.Span(u.IsActive ? "Active" : "Inactive")
                    .Style("color", u.IsActive ? "green" : "red")
            ])
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("<strong>John</strong>", result);
        Assert.Contains("<strong>Jane</strong>", result);
        Assert.Contains("<a href=\"mailto:john@example.com\">john@example.com</a>", result);
        Assert.Contains("<a href=\"mailto:jane@example.com\">jane@example.com</a>", result);
        Assert.Contains("<span style=\"color: green\">Active</span>", result);
        Assert.Contains("<span style=\"color: red\">Inactive</span>", result);
    }

    [Fact]
    public async Task Table_ColumnDefinitions_Works()
    {
        // Arrange
        var users = new[]
        {
            new User("John", "john@example.com", true),
            new User("Jane", "jane@example.com", false)
        };

        // Act
        var table = Html.Table(users)
            .Column("Name", u => u.Name)
            .Column("Email", u => Html.A($"mailto:{u.Email}", u.Email))
            .Column("Status", u => u.IsActive ? "Active" : "Inactive")
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("<th>Name</th>", result);
        Assert.Contains("<th>Email</th>", result);
        Assert.Contains("<th>Status</th>", result);
        Assert.Contains("<td>John</td>", result);
        Assert.Contains("<td>Jane</td>", result);
        Assert.Contains("<a href=\"mailto:john@example.com\">john@example.com</a>", result);
        Assert.Contains("<td>Active</td>", result);
        Assert.Contains("<td>Inactive</td>", result);
    }

    [Fact]
    public async Task Table_WithRowIndex_Works()
    {
        // Arrange
        var items = new[]
        {
            new Item("First"),
            new Item("Second"),
            new Item("Third")
        };

        // Act
        var table = Html.Table(items)
            .Header("#", "Value")
            .Row((item, index) => [(index + 1).ToString(), item.Name])
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("<td>1</td>", result);
        Assert.Contains("<td>First</td>", result);
        Assert.Contains("<td>2</td>", result);
        Assert.Contains("<td>Second</td>", result);
        Assert.Contains("<td>3</td>", result);
        Assert.Contains("<td>Third</td>", result);
    }

    [Fact]
    public async Task Table_WithRowClassSelector_Works()
    {
        // Arrange
        var users = new[]
        {
            new User("John", "john@example.com", true),
            new User("Jane", "jane@example.com", false)
        };

        // Act
        var table = Html.Table(users)
            .Header("Name")
            .Row(u => [u.Name])
            .RowClass(u => u.IsActive ? "active-row" : "inactive-row")
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("class=\"active-row\"", result);
        Assert.Contains("class=\"inactive-row\"", result);
    }

    [Fact]
    public async Task Table_WithCaption_Works()
    {
        // Arrange
        var users = new[] { new User("John", "john@example.com", true) };

        // Act
        var table = Html.Table(users)
            .Caption("User List")
            .Column("Name", u => u.Name)
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("<caption>User List</caption>", result);
    }

    [Fact]
    public async Task Table_WithStyling_Works()
    {
        // Arrange
        var users = new[] { new User("John", "john@example.com", true) };

        // Act
        var table = Html.Table(users)
            .Class("table", "table-striped")
            .Id("users-table")
            .Style("width", "100%")
            .Column("Name", u => u.Name)
            .Render();

        var result = await table.RenderAsync(_services);

        // Assert
        Assert.Contains("class=\"table table-striped\"", result);
        Assert.Contains("id=\"users-table\"", result);
        Assert.Contains("style=\"width: 100%\"", result);
    }

    #endregion

    #region Select Collection Tests

    [Fact]
    public async Task Select_FromCollection_Works()
    {
        // Arrange
        var countries = new[]
        {
            new Country("us", "United States"),
            new Country("uk", "United Kingdom"),
            new Country("ca", "Canada")
        };

        // Act
        var select = Html.Select(countries, "country")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .Placeholder("Select a country...")
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("<select", result);
        Assert.Contains("name=\"country\"", result);
        Assert.Contains("<option value=\"\">Select a country...</option>", result);
        Assert.Contains("<option value=\"us\">United States</option>", result);
        Assert.Contains("<option value=\"uk\">United Kingdom</option>", result);
        Assert.Contains("<option value=\"ca\">Canada</option>", result);
    }

    [Fact]
    public async Task Select_WithSelectedValue_Works()
    {
        // Arrange
        var countries = new[]
        {
            new Country("us", "United States"),
            new Country("uk", "United Kingdom"),
            new Country("ca", "Canada")
        };

        // Act
        var select = Html.Select(countries, "country")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .SelectedValue("uk")
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"uk\" selected>United Kingdom</option>", result);
        Assert.DoesNotContain("<option value=\"us\" selected", result);
        Assert.DoesNotContain("<option value=\"ca\" selected", result);
    }

    [Fact]
    public async Task Select_WithSelectedPredicate_Works()
    {
        // Arrange
        var countries = new[]
        {
            new Country("us", "United States"),
            new Country("uk", "United Kingdom"),
            new Country("ca", "Canada")
        };
        var currentCode = "ca";

        // Act
        var select = Html.Select(countries, "country")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .Selected(c => c.Code == currentCode)
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"ca\" selected>Canada</option>", result);
        Assert.DoesNotContain("<option value=\"us\" selected", result);
        Assert.DoesNotContain("<option value=\"uk\" selected", result);
    }

    [Fact]
    public async Task Select_WithGroupBy_Works()
    {
        // Arrange
        var cars = new[]
        {
            new Car("volvo", "Volvo", "Swedish"),
            new Car("saab", "Saab", "Swedish"),
            new Car("mercedes", "Mercedes", "German"),
            new Car("audi", "Audi", "German")
        };

        // Act
        var select = Html.Select(cars, "car")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .GroupBy(c => c.Country)
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("<optgroup label=\"Swedish\">", result);
        Assert.Contains("<optgroup label=\"German\">", result);
        Assert.Contains("<option value=\"volvo\">Volvo</option>", result);
        Assert.Contains("<option value=\"saab\">Saab</option>", result);
        Assert.Contains("<option value=\"mercedes\">Mercedes</option>", result);
        Assert.Contains("<option value=\"audi\">Audi</option>", result);
    }

    [Fact]
    public async Task Select_WithDisabledOption_Works()
    {
        // Arrange
        var plans = new[]
        {
            new Plan("free", "Free", false),
            new Plan("pro", "Pro", false),
            new Plan("enterprise", "Enterprise", true)
        };

        // Act
        var select = Html.Select(plans, "plan")
            .Value(p => p.Code)
            .Text(p => p.Name)
            .DisabledOption(p => p.RequiresPremium)
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"free\">Free</option>", result);
        Assert.Contains("<option value=\"pro\">Pro</option>", result);
        Assert.Contains("<option value=\"enterprise\" disabled>Enterprise</option>", result);
    }

    [Fact]
    public async Task Select_ManualWithOptGroups_Works()
    {
        // Act
        var select = Html.Select("vehicle")
            .OptGroup("Swedish Cars")
                .Option("volvo", "Volvo")
                .Option("saab", "Saab")
            .EndGroup()
            .OptGroup("German Cars")
                .Option("mercedes", "Mercedes")
                .Option("audi", "Audi")
            .EndGroup()
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("<optgroup label=\"Swedish Cars\">", result);
        Assert.Contains("<option value=\"volvo\">Volvo</option>", result);
        Assert.Contains("<option value=\"saab\">Saab</option>", result);
        Assert.Contains("</optgroup>", result);
        Assert.Contains("<optgroup label=\"German Cars\">", result);
        Assert.Contains("<option value=\"mercedes\">Mercedes</option>", result);
        Assert.Contains("<option value=\"audi\">Audi</option>", result);
    }

    [Fact]
    public async Task Select_WithAllAttributes_Works()
    {
        // Arrange
        var items = new[] { new Country("a", "Option A") };

        // Act
        var select = Html.Select(items, "test")
            .Id("test-select")
            .Class("form-control")
            .Required()
            .Multiple()
            .Size(5)
            .Value(x => x.Code)
            .Text(x => x.Name)
            .Render();

        var result = await select.RenderAsync(_services);

        // Assert
        Assert.Contains("id=\"test-select\"", result);
        Assert.Contains("class=\"form-control\"", result);
        Assert.Contains("name=\"test\"", result);
        Assert.Contains("required", result);
        Assert.Contains("multiple", result);
        Assert.Contains("size=\"5\"", result);
    }

    #endregion

    #region List Collection Tests

    [Fact]
    public async Task Ul_WithStringSelector_Works()
    {
        // Arrange
        var items = new[] { "Apple", "Banana", "Cherry" };

        // Act
        var ul = Html.Ul(items, x => x).Render();
        var result = await ul.RenderAsync(_services);

        // Assert
        Assert.Contains("<ul>", result);
        Assert.Contains("<li>Apple</li>", result);
        Assert.Contains("<li>Banana</li>", result);
        Assert.Contains("<li>Cherry</li>", result);
        Assert.Contains("</ul>", result);
    }

    [Fact]
    public async Task Ul_WithElementSelector_Works()
    {
        // Arrange
        var items = new[] { "Apple", "Banana", "Cherry" };

        // Act
        var ul = Html.Ul(items, item => Html.Strong(item)).Render();
        var result = await ul.RenderAsync(_services);

        // Assert
        Assert.Contains("<ul>", result);
        Assert.Contains("<li><strong>Apple</strong></li>", result);
        Assert.Contains("<li><strong>Banana</strong></li>", result);
        Assert.Contains("<li><strong>Cherry</strong></li>", result);
    }

    [Fact]
    public async Task Ol_WithStringSelector_Works()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        // Act
        var ol = Html.Ol(items, x => x).Render();
        var result = await ol.RenderAsync(_services);

        // Assert
        Assert.Contains("<ol>", result);
        Assert.Contains("<li>First</li>", result);
        Assert.Contains("<li>Second</li>", result);
        Assert.Contains("<li>Third</li>", result);
        Assert.Contains("</ol>", result);
    }

    [Fact]
    public async Task Ol_WithElementSelector_Works()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        // Act
        var ol = Html.Ol(items, item => Html.Em(item)).Render();
        var result = await ol.RenderAsync(_services);

        // Assert
        Assert.Contains("<ol>", result);
        Assert.Contains("<li><em>First</em></li>", result);
        Assert.Contains("<li><em>Second</em></li>", result);
        Assert.Contains("<li><em>Third</em></li>", result);
    }

    #endregion

    #region Html.Each Tests

    [Fact]
    public async Task Each_WithCollection_Works()
    {
        // Arrange
        var products = new[]
        {
            new Product("Widget", "A useful widget"),
            new Product("Gadget", "A cool gadget"),
            new Product("Gizmo", "A fancy gizmo")
        };

        // Act
        var cards = Html.Each(products, p =>
            Html.Div()
                .Class("card")
                .Child(Html.H3(p.Name))
                .Child(Html.P(p.Description)));

        var result = await cards.RenderAsync(_services);

        // Assert
        Assert.Contains("<div class=\"card\">", result);
        Assert.Contains("<h3>Widget</h3>", result);
        Assert.Contains("<p>A useful widget</p>", result);
        Assert.Contains("<h3>Gadget</h3>", result);
        Assert.Contains("<p>A cool gadget</p>", result);
        Assert.Contains("<h3>Gizmo</h3>", result);
        Assert.Contains("<p>A fancy gizmo</p>", result);
    }

    [Fact]
    public async Task Each_WithIndex_Works()
    {
        // Arrange
        var items = new[] { "A", "B", "C" };

        // Act
        var elements = Html.Each(items, (item, index) =>
            Html.Div($"{index + 1}. {item}"));

        var result = await elements.RenderAsync(_services);

        // Assert
        Assert.Contains("<div>1. A</div>", result);
        Assert.Contains("<div>2. B</div>", result);
        Assert.Contains("<div>3. C</div>", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompleteForm_WithTableAndSelect_Works()
    {
        // Arrange
        var users = new[]
        {
            new User("John", "john@example.com", true),
            new User("Jane", "jane@example.com", false)
        };

        var roles = new[]
        {
            new Role("admin", "Administrator"),
            new Role("editor", "Editor"),
            new Role("viewer", "Viewer")
        };

        // Act
        var form = Html.Div()
            .Class("container")
            .Child(Html.H1("User Management"))
            .Child(Html.Table(users)
                .Class("table")
                .Column("Name", u => Html.Strong(u.Name))
                .Column("Email", u => u.Email)
                .Column("Status", u => u.IsActive ? "Active" : "Inactive"))
            .Child(Html.Form("/add-user", "POST")
                .Child(Html.Label("Role:", "role"))
                .Child(Html.Select(roles, "role")
                    .Id("role")
                    .Value(r => r.Code)
                    .Text(r => r.Name)
                    .Placeholder("Select role..."))
                .Child(Html.Button("Add User", "submit")))
            .Render();

        var result = await form.RenderAsync(_services);

        // Assert - Container structure
        Assert.Contains("<div class=\"container\">", result);
        Assert.Contains("<h1>User Management</h1>", result);

        // Assert - Table
        Assert.Contains("<table class=\"table\">", result);
        Assert.Contains("<strong>John</strong>", result);
        Assert.Contains("<strong>Jane</strong>", result);

        // Assert - Form
        Assert.Contains("<form", result);
        Assert.Contains("action=\"/add-user\"", result);
        Assert.Contains("method=\"POST\"", result);

        // Assert - Select
        Assert.Contains("<select", result);
        Assert.Contains("name=\"role\"", result);
        Assert.Contains("<option value=\"\">Select role...</option>", result);
        Assert.Contains("<option value=\"admin\">Administrator</option>", result);
        Assert.Contains("<option value=\"editor\">Editor</option>", result);
        Assert.Contains("<option value=\"viewer\">Viewer</option>", result);

        // Assert - Button
        Assert.Contains("<button", result);
        Assert.Contains("Add User", result);
    }

    [Fact]
    public async Task NestedCollections_Work()
    {
        // Arrange
        var categories = new[]
        {
            new Category("Electronics", new[] { "Phone", "Laptop", "Tablet" }),
            new Category("Clothing", new[] { "Shirt", "Pants", "Jacket" })
        };

        // Act
        var fragment = Html.Div()
            .Children(categories.Select(cat =>
                Html.Div()
                    .Class("category")
                    .Child(Html.H2(cat.Name))
                    .Child(Html.Ul(cat.Items, item => item))))
            .Render();

        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<h2>Electronics</h2>", result);
        Assert.Contains("<li>Phone</li>", result);
        Assert.Contains("<li>Laptop</li>", result);
        Assert.Contains("<li>Tablet</li>", result);
        Assert.Contains("<h2>Clothing</h2>", result);
        Assert.Contains("<li>Shirt</li>", result);
        Assert.Contains("<li>Pants</li>", result);
        Assert.Contains("<li>Jacket</li>", result);
    }

    #endregion

    #region Test Models

    private record User(string Name, string Email, bool IsActive);
    private record Country(string Code, string Name);
    private record Car(string Code, string Name, string Country);
    private record Plan(string Code, string Name, bool RequiresPremium);
    private record Item(string Name);
    private record Product(string Name, string Description);
    private record Role(string Code, string Name);
    private record Category(string Name, string[] Items);

    #endregion
}
