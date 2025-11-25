namespace RazorHelpers.Tests;

public class TableBuilderTests
{
    private readonly IServiceProvider _services;

    public TableBuilderTests()
    {
        _services = TestServiceProvider.Create();
    }

    [Fact]
    public async Task Table_WithHeaderAndRows_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Table()
            .Header("Name", "Email")
            .Row("John", "john@example.com")
            .Row("Jane", "jane@example.com")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<table>", result);
        Assert.Contains("<thead>", result);
        Assert.Contains("<th>Name</th>", result);
        Assert.Contains("<th>Email</th>", result);
        Assert.Contains("<tbody>", result);
        Assert.Contains("<td>John</td>", result);
        Assert.Contains("<td>john@example.com</td>", result);
        Assert.Contains("<td>Jane</td>", result);
        Assert.Contains("<td>jane@example.com</td>", result);
        Assert.Contains("</table>", result);
    }

    [Fact]
    public async Task Table_WithCaption_RendersCaption()
    {
        // Arrange
        var fragment = Html.Table()
            .Caption("User List")
            .Header("Name")
            .Row("John")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<caption>User List</caption>", result);
    }

    [Fact]
    public async Task Table_WithClassAndStyle_RendersAttributes()
    {
        // Arrange
        var fragment = Html.Table()
            .Class("table", "table-striped")
            .Style("width", "100%")
            .Id("users-table")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("class=\"table table-striped\"", result);
        Assert.Contains("style=\"width: 100%\"", result);
        Assert.Contains("id=\"users-table\"", result);
    }

    [Fact]
    public async Task Table_WithElementRows_RendersCustomContent()
    {
        // Arrange
        var fragment = Html.Table()
            .Header("Name", "Status")
            .Row(Html.Strong("John"), Html.Span("Active").Class("badge"))
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<strong>John</strong>", result);
        Assert.Contains("<span class=\"badge\">Active</span>", result);
    }

    [Fact]
    public async Task TableGeneric_WithCollection_RendersAllItems()
    {
        // Arrange
        var users = new[]
        {
            new TestUser("John", "john@example.com"),
            new TestUser("Jane", "jane@example.com"),
            new TestUser("Bob", "bob@example.com")
        };

        var fragment = Html.Table(users)
            .Header("Name", "Email")
            .Row(u => [u.Name, u.Email])
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<td>John</td>", result);
        Assert.Contains("<td>john@example.com</td>", result);
        Assert.Contains("<td>Jane</td>", result);
        Assert.Contains("<td>jane@example.com</td>", result);
        Assert.Contains("<td>Bob</td>", result);
        Assert.Contains("<td>bob@example.com</td>", result);
    }

    [Fact]
    public async Task TableGeneric_WithElementRowSelector_RendersCustomElements()
    {
        // Arrange
        var users = new[]
        {
            new TestUser("John", "john@example.com", true),
            new TestUser("Jane", "jane@example.com", false)
        };

        var fragment = Html.Table(users)
            .Header("Name", "Email", "Status")
            .Row(u => [
                Html.Strong(u.Name),
                Html.A($"mailto:{u.Email}", u.Email),
                Html.Span(u.IsActive ? "Active" : "Inactive")
                    .ClassIf("text-success", u.IsActive)
                    .ClassIf("text-danger", !u.IsActive)
            ])
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<strong>John</strong>", result);
        Assert.Contains("<a href=\"mailto:john@example.com\">john@example.com</a>", result);
        Assert.Contains("class=\"text-success\"", result);
        Assert.Contains("class=\"text-danger\"", result);
    }

    [Fact]
    public async Task TableGeneric_WithColumnDefinitions_RendersCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser("John", "john@example.com"),
            new TestUser("Jane", "jane@example.com")
        };

        var fragment = Html.Table(users)
            .Column("Name", u => u.Name)
            .Column("Email", u => u.Email)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<th>Name</th>", result);
        Assert.Contains("<th>Email</th>", result);
        Assert.Contains("<td>John</td>", result);
        Assert.Contains("<td>john@example.com</td>", result);
    }

    [Fact]
    public async Task TableGeneric_WithColumnElementSelector_RendersCustomElements()
    {
        // Arrange
        var users = new[]
        {
            new TestUser("John", "john@example.com", true)
        };

        var fragment = Html.Table(users)
            .Column("Name", u => Html.Strong(u.Name))
            .Column("Status", u => Html.Span(u.IsActive ? "Active" : "Inactive")
                .ClassIf("active", u.IsActive))
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<strong>John</strong>", result);
        Assert.Contains("class=\"active\"", result);
    }

    [Fact]
    public async Task TableGeneric_WithRowClass_AddsClassToRows()
    {
        // Arrange
        var users = new[]
        {
            new TestUser("John", "john@example.com", true),
            new TestUser("Jane", "jane@example.com", false)
        };

        var fragment = Html.Table(users)
            .Header("Name")
            .Row(u => [u.Name])
            .RowClass(u => u.IsActive ? "row-active" : "row-inactive")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("class=\"row-active\"", result);
        Assert.Contains("class=\"row-inactive\"", result);
    }

    [Fact]
    public async Task TableGeneric_WithRowIndex_ProviesIndexToSelector()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        var fragment = Html.Table(items)
            .Header("#", "Value")
            .Row((item, index) => [(index + 1).ToString(), item])
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<td>1</td>", result);
        Assert.Contains("<td>First</td>", result);
        Assert.Contains("<td>2</td>", result);
        Assert.Contains("<td>Second</td>", result);
        Assert.Contains("<td>3</td>", result);
        Assert.Contains("<td>Third</td>", result);
    }

    [Fact]
    public async Task Table_ImplicitConversion_Works()
    {
        // Arrange
        Microsoft.AspNetCore.Components.RenderFragment fragment = Html.Table()
            .Header("Col1")
            .Row("Value1");

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<table>", result);
    }

    private record TestUser(string Name, string Email, bool IsActive = false);
}
