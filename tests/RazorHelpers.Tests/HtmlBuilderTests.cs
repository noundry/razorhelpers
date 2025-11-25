using Microsoft.AspNetCore.Components;

namespace RazorHelpers.Tests;

public class HtmlBuilderTests
{
    private readonly IServiceProvider _services;

    public HtmlBuilderTests()
    {
        _services = TestServiceProvider.Create();
    }

    [Fact]
    public async Task Div_WithContent_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Div("Hello World").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<div>Hello World</div>", result);
    }

    [Fact]
    public async Task Div_WithClass_RendersClassAttribute()
    {
        // Arrange
        var fragment = Html.Div("Content").Class("container", "mt-4").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("class=\"container mt-4\"", result);
        Assert.Contains("<div", result);
    }

    [Fact]
    public async Task Element_WithId_RendersIdAttribute()
    {
        // Arrange
        var fragment = Html.Div().Id("my-div").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("id=\"my-div\"", result);
    }

    [Fact]
    public async Task Element_WithStyles_RendersStyleAttribute()
    {
        // Arrange
        var fragment = Html.Div()
            .Style("color", "red")
            .Style("font-size", "16px")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("style=\"color: red; font-size: 16px\"", result);
    }

    [Fact]
    public async Task Element_WithDataAttribute_RendersDataAttribute()
    {
        // Arrange
        var fragment = Html.Div().Data("id", "123").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("data-id=\"123\"", result);
    }

    [Fact]
    public async Task Element_WithNestedChildren_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Div()
            .Class("card")
            .Child(Html.H1("Title"))
            .Child(Html.P("Content"))
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<div", result);
        Assert.Contains("class=\"card\"", result);
        Assert.Contains("<h1>Title</h1>", result);
        Assert.Contains("<p>Content</p>", result);
    }

    [Fact]
    public async Task AllHeadings_RenderCorrectly()
    {
        // Arrange
        var fragment = Html.Fragment(
            Html.H1("Heading 1"),
            Html.H2("Heading 2"),
            Html.H3("Heading 3"),
            Html.H4("Heading 4"),
            Html.H5("Heading 5"),
            Html.H6("Heading 6")
        );

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<h1>Heading 1</h1>", result);
        Assert.Contains("<h2>Heading 2</h2>", result);
        Assert.Contains("<h3>Heading 3</h3>", result);
        Assert.Contains("<h4>Heading 4</h4>", result);
        Assert.Contains("<h5>Heading 5</h5>", result);
        Assert.Contains("<h6>Heading 6</h6>", result);
    }

    [Fact]
    public async Task Anchor_WithHref_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.A("https://example.com", "Click here").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<a href=\"https://example.com\">Click here</a>", result);
    }

    [Fact]
    public async Task Img_WithSrcAndAlt_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Img("/images/test.png", "Test image").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("src=\"/images/test.png\"", result);
        Assert.Contains("alt=\"Test image\"", result);
        Assert.Contains("<img", result);
    }

    [Fact]
    public async Task ClassIf_WhenConditionTrue_AddsClass()
    {
        // Arrange
        var fragment = Html.Div()
            .ClassIf("active", true)
            .ClassIf("hidden", false)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("class=\"active\"", result);
        Assert.DoesNotContain("hidden", result);
    }

    [Fact]
    public async Task Form_WithInputs_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Form("/submit", "POST")
            .Child(Html.Label("Name:", "name"))
            .Child(Html.Div().Content(Html.Input("text", "name").Render()))
            .Child(Html.Button("Submit", "submit"))
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<form", result);
        Assert.Contains("action=\"/submit\"", result);
        Assert.Contains("method=\"POST\"", result);
        Assert.Contains("<label", result);
        Assert.Contains("for=\"name\"", result);
        Assert.Contains("<input", result);
        Assert.Contains("type=\"text\"", result);
        Assert.Contains("name=\"name\"", result);
        Assert.Contains("<button", result);
    }

    [Fact]
    public async Task RawMarkup_RendersUnencoded()
    {
        // Arrange
        var fragment = Html.Div().Raw("<strong>Bold</strong>").Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<strong>Bold</strong>", result);
    }

    [Fact]
    public async Task Ul_WithItems_RendersListCorrectly()
    {
        // Arrange
        var items = new[] { "Apple", "Banana", "Cherry" };
        var fragment = Html.Ul(items, x => x).Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<ul>", result);
        Assert.Contains("<li>Apple</li>", result);
        Assert.Contains("<li>Banana</li>", result);
        Assert.Contains("<li>Cherry</li>", result);
        Assert.Contains("</ul>", result);
    }

    [Fact]
    public async Task Each_WithCollection_RendersMultipleElements()
    {
        // Arrange
        var items = new[] { "Item 1", "Item 2", "Item 3" };
        var fragment = Html.Each(items, item => Html.P(item));

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<p>Item 1</p>", result);
        Assert.Contains("<p>Item 2</p>", result);
        Assert.Contains("<p>Item 3</p>", result);
    }

    [Fact]
    public async Task ImplicitConversion_ToRenderFragment_Works()
    {
        // Arrange
        RenderFragment fragment = Html.Div("Implicit conversion");

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<div>Implicit conversion</div>", result);
    }

    [Fact]
    public async Task SemanticElements_RenderCorrectly()
    {
        // Arrange
        var fragment = Html.Fragment(
            Html.Header("Header"),
            Html.Nav("Navigation"),
            Html.Main("Main content"),
            Html.Article("Article"),
            Html.Section("Section"),
            Html.Aside("Sidebar"),
            Html.Footer("Footer")
        );

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<header>Header</header>", result);
        Assert.Contains("<nav>Navigation</nav>", result);
        Assert.Contains("<main>Main content</main>", result);
        Assert.Contains("<article>Article</article>", result);
        Assert.Contains("<section>Section</section>", result);
        Assert.Contains("<aside>Sidebar</aside>", result);
        Assert.Contains("<footer>Footer</footer>", result);
    }

    [Fact]
    public async Task TextFormattingElements_RenderCorrectly()
    {
        // Arrange
        var fragment = Html.Fragment(
            Html.Strong("Bold"),
            Html.Em("Italic"),
            Html.Code("Code"),
            Html.Pre("Preformatted"),
            Html.Small("Small"),
            Html.Mark("Highlighted")
        );

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<strong>Bold</strong>", result);
        Assert.Contains("<em>Italic</em>", result);
        Assert.Contains("<code>Code</code>", result);
        Assert.Contains("<pre>Preformatted</pre>", result);
        Assert.Contains("<small>Small</small>", result);
        Assert.Contains("<mark>Highlighted</mark>", result);
    }

    [Fact]
    public async Task VoidElements_RenderAsSelfClosing()
    {
        // Arrange
        var fragment = Html.Fragment(
            Html.Div().Child(Html.Br()),
            Html.Div().Child(Html.Hr())
        );

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        // Blazor renders void elements as self-closing tags (e.g., <br />)
        Assert.Contains("<br", result);
        Assert.Contains("<hr", result);
    }

    [Fact]
    public async Task Attrs_Dictionary_AddsMultipleAttributes()
    {
        // Arrange
        var attrs = new Dictionary<string, object?>
        {
            ["data-value"] = "123",
            ["aria-label"] = "Test label"
        };
        var fragment = Html.Div().Attrs(attrs).Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("data-value=\"123\"", result);
        Assert.Contains("aria-label=\"Test label\"", result);
    }

    [Fact]
    public async Task Styles_Dictionary_AddsMultipleStyles()
    {
        // Arrange
        var styles = new Dictionary<string, string>
        {
            ["color"] = "blue",
            ["margin"] = "10px"
        };
        var fragment = Html.Div().Styles(styles).Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("style=\"", result);
        Assert.Contains("color: blue", result);
        Assert.Contains("margin: 10px", result);
    }
}

