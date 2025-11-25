using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RazorHelpers;

/// <summary>
/// Represents an HTML element that can be built fluently and rendered as a RenderFragment.
/// </summary>
public class HtmlElement
{
    private readonly string _tagName;
    private readonly List<HtmlElement> _children = [];
    private readonly Dictionary<string, object?> _attributes = [];
    private readonly List<string> _cssClasses = [];
    private readonly Dictionary<string, string> _styles = [];
    private string? _textContent;
    private string? _rawMarkup;
    private RenderFragment? _childFragment;

    /// <summary>
    /// Creates a new HTML element with the specified tag name.
    /// </summary>
    /// <param name="tagName">The HTML tag name (e.g., "div", "span", "p").</param>
    public HtmlElement(string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        _tagName = tagName;
    }

    /// <summary>
    /// Creates a new HTML element with the specified tag name and text content.
    /// </summary>
    /// <param name="tagName">The HTML tag name.</param>
    /// <param name="content">The text content of the element.</param>
    public HtmlElement(string tagName, string? content) : this(tagName)
    {
        _textContent = content;
    }

    /// <summary>
    /// Adds one or more CSS classes to the element.
    /// </summary>
    /// <param name="classes">The CSS class names to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Class(params string[] classes)
    {
        _cssClasses.AddRange(classes.Where(c => !string.IsNullOrWhiteSpace(c)));
        return this;
    }

    /// <summary>
    /// Adds a CSS class conditionally.
    /// </summary>
    /// <param name="className">The CSS class name.</param>
    /// <param name="condition">Whether to add the class.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement ClassIf(string className, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(className))
        {
            _cssClasses.Add(className);
        }
        return this;
    }

    /// <summary>
    /// Sets the id attribute of the element.
    /// </summary>
    /// <param name="id">The id value.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Id(string id)
    {
        _attributes["id"] = id;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the element.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Attr(string name, object? value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple attributes to the element.
    /// </summary>
    /// <param name="attributes">Dictionary of attribute name-value pairs.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Attrs(IDictionary<string, object?> attributes)
    {
        foreach (var (key, value) in attributes)
        {
            _attributes[key] = value;
        }
        return this;
    }

    /// <summary>
    /// Adds a data-* attribute to the element.
    /// </summary>
    /// <param name="name">The data attribute name (without the data- prefix).</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Data(string name, object? value)
    {
        _attributes[$"data-{name}"] = value;
        return this;
    }

    /// <summary>
    /// Adds an inline style to the element.
    /// </summary>
    /// <param name="property">The CSS property name.</param>
    /// <param name="value">The CSS value.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Style(string property, string value)
    {
        _styles[property] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple inline styles to the element.
    /// </summary>
    /// <param name="styles">Dictionary of CSS property-value pairs.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Styles(IDictionary<string, string> styles)
    {
        foreach (var (property, value) in styles)
        {
            _styles[property] = value;
        }
        return this;
    }

    /// <summary>
    /// Sets the text content of the element (HTML-encoded).
    /// </summary>
    /// <param name="content">The text content.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Text(string? content)
    {
        _textContent = content;
        return this;
    }

    /// <summary>
    /// Sets raw HTML markup as the content (not encoded).
    /// </summary>
    /// <param name="markup">The raw HTML markup.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Raw(string? markup)
    {
        _rawMarkup = markup;
        return this;
    }

    /// <summary>
    /// Sets a RenderFragment as the child content.
    /// </summary>
    /// <param name="fragment">The RenderFragment to render as child content.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Content(RenderFragment fragment)
    {
        _childFragment = fragment;
        return this;
    }

    /// <summary>
    /// Adds a child element.
    /// </summary>
    /// <param name="child">The child element to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Child(HtmlElement child)
    {
        _children.Add(child);
        return this;
    }

    /// <summary>
    /// Adds a void element as a child (e.g., input, br, hr, img).
    /// </summary>
    /// <param name="child">The void element to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Child(HtmlVoidElement child)
    {
        _childFragment = CombineFragments(_childFragment, child.Render());
        return this;
    }

    /// <summary>
    /// Adds a table builder as a child.
    /// </summary>
    /// <param name="table">The table builder to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Child(TableBuilder table)
    {
        _childFragment = CombineFragments(_childFragment, table.Render());
        return this;
    }

    /// <summary>
    /// Adds a generic table builder as a child.
    /// </summary>
    /// <typeparam name="T">The type of items in the table.</typeparam>
    /// <param name="table">The table builder to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Child<T>(TableBuilder<T> table)
    {
        _childFragment = CombineFragments(_childFragment, table.Render());
        return this;
    }

    /// <summary>
    /// Adds a select builder as a child.
    /// </summary>
    /// <param name="select">The select builder to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Child(SelectBuilder select)
    {
        _childFragment = CombineFragments(_childFragment, select.Render());
        return this;
    }

    /// <summary>
    /// Adds a generic select builder as a child.
    /// </summary>
    /// <typeparam name="T">The type of items in the select.</typeparam>
    /// <param name="select">The select builder to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Child<T>(SelectBuilder<T> select)
    {
        _childFragment = CombineFragments(_childFragment, select.Render());
        return this;
    }

    /// <summary>
    /// Adds multiple child elements.
    /// </summary>
    /// <param name="children">The child elements to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Children(params HtmlElement[] children)
    {
        _children.AddRange(children);
        return this;
    }

    /// <summary>
    /// Adds multiple child elements from a collection.
    /// </summary>
    /// <param name="children">The child elements to add.</param>
    /// <returns>The current element for method chaining.</returns>
    public HtmlElement Children(IEnumerable<HtmlElement> children)
    {
        _children.AddRange(children);
        return this;
    }

    private static RenderFragment? CombineFragments(RenderFragment? existing, RenderFragment newFragment)
    {
        if (existing is null)
            return newFragment;

        return builder =>
        {
            existing(builder);
            newFragment(builder);
        };
    }

    /// <summary>
    /// Renders the element and its children to a RenderFragment.
    /// </summary>
    /// <returns>A RenderFragment representing this element.</returns>
    public RenderFragment Render() => builder =>
    {
        var seq = 0;
        BuildRenderTree(builder, ref seq);
    };

    /// <summary>
    /// Implicitly converts an HtmlElement to a RenderFragment.
    /// </summary>
    /// <param name="element">The element to convert.</param>
    public static implicit operator RenderFragment(HtmlElement element) => element.Render();

    internal void BuildRenderTree(RenderTreeBuilder builder, ref int seq)
    {
        builder.OpenElement(seq++, _tagName);

        // Add id if present
        if (_attributes.TryGetValue("id", out var id))
        {
            builder.AddAttribute(seq++, "id", id);
        }

        // Add classes
        if (_cssClasses.Count > 0)
        {
            builder.AddAttribute(seq++, "class", string.Join(" ", _cssClasses));
        }

        // Add styles
        if (_styles.Count > 0)
        {
            builder.AddAttribute(seq++, "style",
                string.Join("; ", _styles.Select(s => $"{s.Key}: {s.Value}")));
        }

        // Add other attributes
        foreach (var (name, value) in _attributes.Where(a => a.Key != "id"))
        {
            builder.AddAttribute(seq++, name, value);
        }

        // Add content
        if (_textContent is not null)
        {
            builder.AddContent(seq++, _textContent);
        }

        if (_rawMarkup is not null)
        {
            builder.AddMarkupContent(seq++, _rawMarkup);
        }

        if (_childFragment is not null)
        {
            builder.AddContent(seq++, _childFragment);
        }

        // Add children
        foreach (var child in _children)
        {
            child.BuildRenderTree(builder, ref seq);
        }

        builder.CloseElement();
    }
}

/// <summary>
/// Static entry point for building HTML elements fluently.
/// </summary>
/// <example>
/// <code>
/// // Simple element
/// var fragment = Html.Div("Hello World").Class("greeting").Render();
///
/// // Nested elements
/// var card = Html.Div()
///     .Class("card")
///     .Child(Html.H1("Title"))
///     .Child(Html.P("Content"))
///     .Render();
///
/// // Table with collection
/// var table = Html.Table&lt;User&gt;(users)
///     .Header("Name", "Email", "Role")
///     .Row(u => [u.Name, u.Email, u.Role])
///     .Render();
/// </code>
/// </example>
public static class Html
{
    // Document structure
    /// <summary>Creates a &lt;div&gt; element.</summary>
    public static HtmlElement Div(string? content = null) => new("div", content);

    /// <summary>Creates a &lt;span&gt; element.</summary>
    public static HtmlElement Span(string? content = null) => new("span", content);

    /// <summary>Creates a &lt;p&gt; element.</summary>
    public static HtmlElement P(string? content = null) => new("p", content);

    /// <summary>Creates a &lt;section&gt; element.</summary>
    public static HtmlElement Section(string? content = null) => new("section", content);

    /// <summary>Creates an &lt;article&gt; element.</summary>
    public static HtmlElement Article(string? content = null) => new("article", content);

    /// <summary>Creates a &lt;header&gt; element.</summary>
    public static HtmlElement Header(string? content = null) => new("header", content);

    /// <summary>Creates a &lt;footer&gt; element.</summary>
    public static HtmlElement Footer(string? content = null) => new("footer", content);

    /// <summary>Creates a &lt;main&gt; element.</summary>
    public static HtmlElement Main(string? content = null) => new("main", content);

    /// <summary>Creates a &lt;nav&gt; element.</summary>
    public static HtmlElement Nav(string? content = null) => new("nav", content);

    /// <summary>Creates an &lt;aside&gt; element.</summary>
    public static HtmlElement Aside(string? content = null) => new("aside", content);

    // Headings
    /// <summary>Creates an &lt;h1&gt; element.</summary>
    public static HtmlElement H1(string? content = null) => new("h1", content);

    /// <summary>Creates an &lt;h2&gt; element.</summary>
    public static HtmlElement H2(string? content = null) => new("h2", content);

    /// <summary>Creates an &lt;h3&gt; element.</summary>
    public static HtmlElement H3(string? content = null) => new("h3", content);

    /// <summary>Creates an &lt;h4&gt; element.</summary>
    public static HtmlElement H4(string? content = null) => new("h4", content);

    /// <summary>Creates an &lt;h5&gt; element.</summary>
    public static HtmlElement H5(string? content = null) => new("h5", content);

    /// <summary>Creates an &lt;h6&gt; element.</summary>
    public static HtmlElement H6(string? content = null) => new("h6", content);

    // Text formatting
    /// <summary>Creates a &lt;strong&gt; element.</summary>
    public static HtmlElement Strong(string? content = null) => new("strong", content);

    /// <summary>Creates an &lt;em&gt; element.</summary>
    public static HtmlElement Em(string? content = null) => new("em", content);

    /// <summary>Creates a &lt;small&gt; element.</summary>
    public static HtmlElement Small(string? content = null) => new("small", content);

    /// <summary>Creates a &lt;mark&gt; element.</summary>
    public static HtmlElement Mark(string? content = null) => new("mark", content);

    /// <summary>Creates a &lt;del&gt; element.</summary>
    public static HtmlElement Del(string? content = null) => new("del", content);

    /// <summary>Creates an &lt;ins&gt; element.</summary>
    public static HtmlElement Ins(string? content = null) => new("ins", content);

    /// <summary>Creates a &lt;sub&gt; element.</summary>
    public static HtmlElement Sub(string? content = null) => new("sub", content);

    /// <summary>Creates a &lt;sup&gt; element.</summary>
    public static HtmlElement Sup(string? content = null) => new("sup", content);

    /// <summary>Creates a &lt;code&gt; element.</summary>
    public static HtmlElement Code(string? content = null) => new("code", content);

    /// <summary>Creates a &lt;pre&gt; element.</summary>
    public static HtmlElement Pre(string? content = null) => new("pre", content);

    /// <summary>Creates a &lt;blockquote&gt; element.</summary>
    public static HtmlElement Blockquote(string? content = null) => new("blockquote", content);

    /// <summary>Creates a &lt;cite&gt; element.</summary>
    public static HtmlElement Cite(string? content = null) => new("cite", content);

    /// <summary>Creates a &lt;abbr&gt; element.</summary>
    public static HtmlElement Abbr(string? content = null) => new("abbr", content);

    /// <summary>Creates a &lt;time&gt; element.</summary>
    public static HtmlElement Time(string? content = null) => new("time", content);

    // Links and media
    /// <summary>Creates an &lt;a&gt; element with the specified href.</summary>
    public static HtmlElement A(string? href = null, string? content = null) =>
        new HtmlElement("a", content).Attr("href", href);

    /// <summary>Creates an &lt;img&gt; element with the specified src.</summary>
    public static HtmlVoidElement Img(string? src = null, string? alt = null) =>
        new HtmlVoidElement("img").Attr("src", src).Attr("alt", alt);

    /// <summary>Creates a &lt;video&gt; element.</summary>
    public static HtmlElement Video(string? src = null) =>
        new HtmlElement("video").Attr("src", src);

    /// <summary>Creates an &lt;audio&gt; element.</summary>
    public static HtmlElement Audio(string? src = null) =>
        new HtmlElement("audio").Attr("src", src);

    /// <summary>Creates a &lt;source&gt; element.</summary>
    public static HtmlVoidElement Source(string? src = null, string? type = null) =>
        new HtmlVoidElement("source").Attr("src", src).Attr("type", type);

    /// <summary>Creates an &lt;iframe&gt; element.</summary>
    public static HtmlElement Iframe(string? src = null) =>
        new HtmlElement("iframe").Attr("src", src);

    /// <summary>Creates a &lt;figure&gt; element.</summary>
    public static HtmlElement Figure() => new("figure");

    /// <summary>Creates a &lt;figcaption&gt; element.</summary>
    public static HtmlElement Figcaption(string? content = null) => new("figcaption", content);

    /// <summary>Creates a &lt;picture&gt; element.</summary>
    public static HtmlElement Picture() => new("picture");

    // Lists
    /// <summary>Creates a &lt;ul&gt; element.</summary>
    public static HtmlElement Ul() => new("ul");

    /// <summary>Creates an &lt;ol&gt; element.</summary>
    public static HtmlElement Ol() => new("ol");

    /// <summary>Creates a &lt;li&gt; element.</summary>
    public static HtmlElement Li(string? content = null) => new("li", content);

    /// <summary>Creates a &lt;dl&gt; element.</summary>
    public static HtmlElement Dl() => new("dl");

    /// <summary>Creates a &lt;dt&gt; element.</summary>
    public static HtmlElement Dt(string? content = null) => new("dt", content);

    /// <summary>Creates a &lt;dd&gt; element.</summary>
    public static HtmlElement Dd(string? content = null) => new("dd", content);

    /// <summary>Creates a &lt;ul&gt; element with list items from a collection.</summary>
    public static HtmlElement Ul<T>(IEnumerable<T> items, Func<T, string> textSelector) =>
        new HtmlElement("ul").Children(items.Select(item => Li(textSelector(item))));

    /// <summary>Creates a &lt;ul&gt; element with custom list items from a collection.</summary>
    public static HtmlElement Ul<T>(IEnumerable<T> items, Func<T, HtmlElement> itemBuilder) =>
        new HtmlElement("ul").Children(items.Select(item => Li().Child(itemBuilder(item))));

    /// <summary>Creates an &lt;ol&gt; element with list items from a collection.</summary>
    public static HtmlElement Ol<T>(IEnumerable<T> items, Func<T, string> textSelector) =>
        new HtmlElement("ol").Children(items.Select(item => Li(textSelector(item))));

    /// <summary>Creates an &lt;ol&gt; element with custom list items from a collection.</summary>
    public static HtmlElement Ol<T>(IEnumerable<T> items, Func<T, HtmlElement> itemBuilder) =>
        new HtmlElement("ol").Children(items.Select(item => Li().Child(itemBuilder(item))));

    // Void elements (self-closing)
    /// <summary>Creates a &lt;br&gt; element.</summary>
    public static HtmlVoidElement Br() => new("br");

    /// <summary>Creates an &lt;hr&gt; element.</summary>
    public static HtmlVoidElement Hr() => new("hr");

    /// <summary>Creates a &lt;wbr&gt; element.</summary>
    public static HtmlVoidElement Wbr() => new("wbr");

    // Forms - basic elements
    /// <summary>Creates a &lt;form&gt; element.</summary>
    public static HtmlElement Form(string? action = null, string? method = null) =>
        new HtmlElement("form").Attr("action", action).Attr("method", method);

    /// <summary>Creates an &lt;input&gt; element.</summary>
    public static HtmlVoidElement Input(string? type = null, string? name = null, string? value = null) =>
        new HtmlVoidElement("input").Attr("type", type).Attr("name", name).Attr("value", value);

    /// <summary>Creates a &lt;textarea&gt; element.</summary>
    public static HtmlElement Textarea(string? name = null, string? content = null) =>
        new HtmlElement("textarea", content).Attr("name", name);

    /// <summary>Creates a &lt;button&gt; element.</summary>
    public static HtmlElement Button(string? content = null, string? type = null) =>
        new HtmlElement("button", content).Attr("type", type ?? "button");

    /// <summary>Creates a &lt;label&gt; element.</summary>
    public static HtmlElement Label(string? content = null, string? forId = null) =>
        new HtmlElement("label", content).Attr("for", forId);

    /// <summary>Creates a &lt;fieldset&gt; element.</summary>
    public static HtmlElement Fieldset() => new("fieldset");

    /// <summary>Creates a &lt;legend&gt; element.</summary>
    public static HtmlElement Legend(string? content = null) => new("legend", content);

    /// <summary>Creates a &lt;datalist&gt; element.</summary>
    public static HtmlElement Datalist() => new("datalist");

    /// <summary>Creates an &lt;output&gt; element.</summary>
    public static HtmlElement Output(string? content = null) => new("output", content);

    /// <summary>Creates a &lt;progress&gt; element.</summary>
    public static HtmlElement Progress(double? value = null, double? max = null) =>
        new HtmlElement("progress").Attr("value", value).Attr("max", max);

    /// <summary>Creates a &lt;meter&gt; element.</summary>
    public static HtmlElement Meter(double? value = null, double? min = null, double? max = null) =>
        new HtmlElement("meter").Attr("value", value).Attr("min", min).Attr("max", max);

    // Table builder
    /// <summary>Creates a new table builder.</summary>
    public static TableBuilder Table() => new();

    /// <summary>Creates a table builder with data from a collection.</summary>
    public static TableBuilder<T> Table<T>(IEnumerable<T> items) => new(items);

    // Select builder
    /// <summary>Creates a new select builder.</summary>
    public static SelectBuilder Select(string? name = null) => new(name);

    /// <summary>Creates a select builder with options from a collection.</summary>
    public static SelectBuilder<T> Select<T>(IEnumerable<T> items, string? name = null) => new(items, name);

    // Generic element
    /// <summary>Creates an element with the specified tag name.</summary>
    public static HtmlElement Element(string tagName, string? content = null) => new(tagName, content);

    /// <summary>Creates a void element with the specified tag name.</summary>
    public static HtmlVoidElement VoidElement(string tagName) => new(tagName);

    // Details/Summary
    /// <summary>Creates a &lt;details&gt; element.</summary>
    public static HtmlElement Details() => new("details");

    /// <summary>Creates a &lt;summary&gt; element.</summary>
    public static HtmlElement Summary(string? content = null) => new("summary", content);

    /// <summary>Creates a &lt;dialog&gt; element.</summary>
    public static HtmlElement Dialog() => new("dialog");

    // Canvas and SVG
    /// <summary>Creates a &lt;canvas&gt; element.</summary>
    public static HtmlElement Canvas(int? width = null, int? height = null) =>
        new HtmlElement("canvas").Attr("width", width).Attr("height", height);

    /// <summary>Creates an &lt;svg&gt; element.</summary>
    public static HtmlElement Svg(int? width = null, int? height = null) =>
        new HtmlElement("svg").Attr("width", width).Attr("height", height);

    // Script and Style
    /// <summary>Creates a &lt;script&gt; element.</summary>
    public static HtmlElement Script(string? src = null, string? content = null) =>
        src is not null ? new HtmlElement("script").Attr("src", src) : new HtmlElement("script", content);

    /// <summary>Creates a &lt;style&gt; element.</summary>
    public static HtmlElement StyleElement(string? content = null) => new("style", content);

    /// <summary>Creates a &lt;link&gt; element.</summary>
    public static HtmlVoidElement Link(string? rel = null, string? href = null) =>
        new HtmlVoidElement("link").Attr("rel", rel).Attr("href", href);

    /// <summary>Creates a &lt;meta&gt; element.</summary>
    public static HtmlVoidElement Meta(string? name = null, string? content = null) =>
        new HtmlVoidElement("meta").Attr("name", name).Attr("content", content);

    // Templating/Container helpers
    /// <summary>Creates a fragment containing multiple elements (no wrapper element).</summary>
    public static RenderFragment Fragment(params HtmlElement[] elements) => builder =>
    {
        var seq = 0;
        foreach (var element in elements)
        {
            element.BuildRenderTree(builder, ref seq);
        }
    };

    /// <summary>Creates a fragment from a collection of elements.</summary>
    public static RenderFragment Fragment(IEnumerable<HtmlElement> elements) => builder =>
    {
        var seq = 0;
        foreach (var element in elements)
        {
            element.BuildRenderTree(builder, ref seq);
        }
    };

    /// <summary>Renders multiple elements from a collection.</summary>
    public static RenderFragment Each<T>(IEnumerable<T> items, Func<T, HtmlElement> builder) =>
        Fragment(items.Select(builder));

    /// <summary>Renders multiple elements from a collection with index.</summary>
    public static RenderFragment Each<T>(IEnumerable<T> items, Func<T, int, HtmlElement> builder) =>
        Fragment(items.Select(builder));
}

/// <summary>
/// Represents a void (self-closing) HTML element like &lt;br&gt;, &lt;hr&gt;, &lt;img&gt;, etc.
/// </summary>
public class HtmlVoidElement
{
    private readonly string _tagName;
    private readonly Dictionary<string, object?> _attributes = [];
    private readonly List<string> _cssClasses = [];
    private readonly Dictionary<string, string> _styles = [];

    /// <summary>
    /// Creates a new void HTML element with the specified tag name.
    /// </summary>
    public HtmlVoidElement(string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        _tagName = tagName;
    }

    /// <summary>Adds one or more CSS classes to the element.</summary>
    public HtmlVoidElement Class(params string[] classes)
    {
        _cssClasses.AddRange(classes.Where(c => !string.IsNullOrWhiteSpace(c)));
        return this;
    }

    /// <summary>Adds a CSS class conditionally.</summary>
    public HtmlVoidElement ClassIf(string className, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(className))
            _cssClasses.Add(className);
        return this;
    }

    /// <summary>Sets the id attribute of the element.</summary>
    public HtmlVoidElement Id(string id)
    {
        _attributes["id"] = id;
        return this;
    }

    /// <summary>Adds an attribute to the element.</summary>
    public HtmlVoidElement Attr(string name, object? value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>Adds multiple attributes to the element.</summary>
    public HtmlVoidElement Attrs(IDictionary<string, object?> attributes)
    {
        foreach (var (key, value) in attributes)
            _attributes[key] = value;
        return this;
    }

    /// <summary>Adds a data-* attribute to the element.</summary>
    public HtmlVoidElement Data(string name, object? value)
    {
        _attributes[$"data-{name}"] = value;
        return this;
    }

    /// <summary>Adds an inline style to the element.</summary>
    public HtmlVoidElement Style(string property, string value)
    {
        _styles[property] = value;
        return this;
    }

    /// <summary>Adds multiple inline styles to the element.</summary>
    public HtmlVoidElement Styles(IDictionary<string, string> styles)
    {
        foreach (var (property, value) in styles)
            _styles[property] = value;
        return this;
    }

    /// <summary>Renders the void element to a RenderFragment.</summary>
    public RenderFragment Render() => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, _tagName);

        if (_attributes.TryGetValue("id", out var id))
            builder.AddAttribute(seq++, "id", id);

        if (_cssClasses.Count > 0)
            builder.AddAttribute(seq++, "class", string.Join(" ", _cssClasses));

        if (_styles.Count > 0)
            builder.AddAttribute(seq++, "style",
                string.Join("; ", _styles.Select(s => $"{s.Key}: {s.Value}")));

        foreach (var (name, value) in _attributes.Where(a => a.Key != "id"))
            builder.AddAttribute(seq++, name, value);

        builder.CloseElement();
    };

    /// <summary>Implicitly converts an HtmlVoidElement to a RenderFragment.</summary>
    public static implicit operator RenderFragment(HtmlVoidElement element) => element.Render();
}
