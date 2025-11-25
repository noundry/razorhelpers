using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RazorHelpers;

/// <summary>
/// Fluent builder for creating HTML tables.
/// </summary>
/// <example>
/// <code>
/// // Simple table
/// var table = Html.Table()
///     .Header("Name", "Email")
///     .Row("John", "john@example.com")
///     .Row("Jane", "jane@example.com")
///     .Render();
///
/// // With styling
/// var styledTable = Html.Table()
///     .Class("table", "table-striped")
///     .Caption("Users")
///     .Header("Name", "Email")
///     .Row("John", "john@example.com")
///     .Render();
/// </code>
/// </example>
public class TableBuilder
{
    private readonly Dictionary<string, object?> _attributes = [];
    private readonly List<string> _cssClasses = [];
    private readonly Dictionary<string, string> _styles = [];
    private string? _caption;
    private readonly List<string> _headerCells = [];
    private readonly List<string[]> _rows = [];
    private readonly List<HtmlElement[]> _elementRows = [];
    private HtmlElement? _thead;
    private HtmlElement? _tbody;
    private HtmlElement? _tfoot;

    /// <summary>Adds one or more CSS classes to the table.</summary>
    public TableBuilder Class(params string[] classes)
    {
        _cssClasses.AddRange(classes.Where(c => !string.IsNullOrWhiteSpace(c)));
        return this;
    }

    /// <summary>Sets the id attribute of the table.</summary>
    public TableBuilder Id(string id)
    {
        _attributes["id"] = id;
        return this;
    }

    /// <summary>Adds an attribute to the table.</summary>
    public TableBuilder Attr(string name, object? value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>Adds an inline style to the table.</summary>
    public TableBuilder Style(string property, string value)
    {
        _styles[property] = value;
        return this;
    }

    /// <summary>Sets the table caption.</summary>
    public TableBuilder Caption(string caption)
    {
        _caption = caption;
        return this;
    }

    /// <summary>Adds header cells to the table.</summary>
    public TableBuilder Header(params string[] cells)
    {
        _headerCells.AddRange(cells);
        return this;
    }

    /// <summary>Adds a custom thead element.</summary>
    public TableBuilder Head(HtmlElement thead)
    {
        _thead = thead;
        return this;
    }

    /// <summary>Adds a custom tbody element.</summary>
    public TableBuilder Body(HtmlElement tbody)
    {
        _tbody = tbody;
        return this;
    }

    /// <summary>Adds a custom tfoot element.</summary>
    public TableBuilder Foot(HtmlElement tfoot)
    {
        _tfoot = tfoot;
        return this;
    }

    /// <summary>Adds a row with text cell values.</summary>
    public TableBuilder Row(params string[] cells)
    {
        _rows.Add(cells);
        return this;
    }

    /// <summary>Adds a row with HtmlElement cells for custom content.</summary>
    public TableBuilder Row(params HtmlElement[] cells)
    {
        _elementRows.Add(cells);
        return this;
    }

    /// <summary>Renders the table to a RenderFragment.</summary>
    public RenderFragment Render() => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "table");

        // Add attributes
        if (_attributes.TryGetValue("id", out var id))
            builder.AddAttribute(seq++, "id", id);

        if (_cssClasses.Count > 0)
            builder.AddAttribute(seq++, "class", string.Join(" ", _cssClasses));

        if (_styles.Count > 0)
            builder.AddAttribute(seq++, "style",
                string.Join("; ", _styles.Select(s => $"{s.Key}: {s.Value}")));

        foreach (var (name, value) in _attributes.Where(a => a.Key != "id"))
            builder.AddAttribute(seq++, name, value);

        // Caption
        if (_caption is not null)
        {
            builder.OpenElement(seq++, "caption");
            builder.AddContent(seq++, _caption);
            builder.CloseElement();
        }

        // Thead
        if (_thead is not null)
        {
            _thead.BuildRenderTree(builder, ref seq);
        }
        else if (_headerCells.Count > 0)
        {
            builder.OpenElement(seq++, "thead");
            builder.OpenElement(seq++, "tr");
            foreach (var cell in _headerCells)
            {
                builder.OpenElement(seq++, "th");
                builder.AddContent(seq++, cell);
                builder.CloseElement();
            }
            builder.CloseElement(); // tr
            builder.CloseElement(); // thead
        }

        // Tbody
        if (_tbody is not null)
        {
            _tbody.BuildRenderTree(builder, ref seq);
        }
        else if (_rows.Count > 0 || _elementRows.Count > 0)
        {
            builder.OpenElement(seq++, "tbody");

            // Text rows
            foreach (var row in _rows)
            {
                builder.OpenElement(seq++, "tr");
                foreach (var cell in row)
                {
                    builder.OpenElement(seq++, "td");
                    builder.AddContent(seq++, cell);
                    builder.CloseElement();
                }
                builder.CloseElement();
            }

            // Element rows
            foreach (var row in _elementRows)
            {
                builder.OpenElement(seq++, "tr");
                foreach (var cell in row)
                {
                    builder.OpenElement(seq++, "td");
                    cell.BuildRenderTree(builder, ref seq);
                    builder.CloseElement();
                }
                builder.CloseElement();
            }

            builder.CloseElement(); // tbody
        }

        // Tfoot
        if (_tfoot is not null)
        {
            _tfoot.BuildRenderTree(builder, ref seq);
        }

        builder.CloseElement(); // table
    };

    /// <summary>Implicitly converts a TableBuilder to a RenderFragment.</summary>
    public static implicit operator RenderFragment(TableBuilder builder) => builder.Render();
}

/// <summary>
/// Fluent builder for creating HTML tables from a collection.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <example>
/// <code>
/// var users = new[] { new User("John", "john@example.com"), new User("Jane", "jane@example.com") };
///
/// // Simple table from collection
/// var table = Html.Table(users)
///     .Header("Name", "Email")
///     .Row(u => [u.Name, u.Email])
///     .Render();
///
/// // With custom cell rendering
/// var table = Html.Table(users)
///     .Header("Name", "Email", "Actions")
///     .Row(u => [
///         Html.Strong(u.Name),
///         Html.A($"mailto:{u.Email}", u.Email),
///         Html.Button("Edit").Class("btn", "btn-sm")
///     ])
///     .Render();
///
/// // Using column definitions
/// var table = Html.Table(users)
///     .Column("Name", u => u.Name)
///     .Column("Email", u => u.Email)
///     .Column("Status", u => Html.Span(u.IsActive ? "Active" : "Inactive")
///         .ClassIf("text-success", u.IsActive)
///         .ClassIf("text-danger", !u.IsActive))
///     .Render();
/// </code>
/// </example>
public class TableBuilder<T>
{
    private readonly IEnumerable<T> _items;
    private readonly Dictionary<string, object?> _attributes = [];
    private readonly List<string> _cssClasses = [];
    private readonly Dictionary<string, string> _styles = [];
    private string? _caption;
    private readonly List<string> _headerCells = [];
    private Func<T, string[]>? _textRowSelector;
    private Func<T, HtmlElement[]>? _elementRowSelector;
    private Func<T, int, string[]>? _textRowSelectorWithIndex;
    private Func<T, int, HtmlElement[]>? _elementRowSelectorWithIndex;
    private readonly List<ColumnDefinition> _columns = [];
    private HtmlElement? _tfoot;
    private Func<T, string>? _rowClassSelector;
    private Func<T, Dictionary<string, object?>>? _rowAttributeSelector;

    internal TableBuilder(IEnumerable<T> items)
    {
        _items = items;
    }

    /// <summary>Adds one or more CSS classes to the table.</summary>
    public TableBuilder<T> Class(params string[] classes)
    {
        _cssClasses.AddRange(classes.Where(c => !string.IsNullOrWhiteSpace(c)));
        return this;
    }

    /// <summary>Sets the id attribute of the table.</summary>
    public TableBuilder<T> Id(string id)
    {
        _attributes["id"] = id;
        return this;
    }

    /// <summary>Adds an attribute to the table.</summary>
    public TableBuilder<T> Attr(string name, object? value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>Adds an inline style to the table.</summary>
    public TableBuilder<T> Style(string property, string value)
    {
        _styles[property] = value;
        return this;
    }

    /// <summary>Sets the table caption.</summary>
    public TableBuilder<T> Caption(string caption)
    {
        _caption = caption;
        return this;
    }

    /// <summary>Adds header cells to the table.</summary>
    public TableBuilder<T> Header(params string[] cells)
    {
        _headerCells.AddRange(cells);
        return this;
    }

    /// <summary>Defines a row selector that returns text values for each cell.</summary>
    public TableBuilder<T> Row(Func<T, string[]> selector)
    {
        _textRowSelector = selector;
        return this;
    }

    /// <summary>Defines a row selector that returns HtmlElements for each cell.</summary>
    public TableBuilder<T> Row(Func<T, HtmlElement[]> selector)
    {
        _elementRowSelector = selector;
        return this;
    }

    /// <summary>Defines a row selector with index that returns text values for each cell.</summary>
    public TableBuilder<T> Row(Func<T, int, string[]> selector)
    {
        _textRowSelectorWithIndex = selector;
        return this;
    }

    /// <summary>Defines a row selector with index that returns HtmlElements for each cell.</summary>
    public TableBuilder<T> Row(Func<T, int, HtmlElement[]> selector)
    {
        _elementRowSelectorWithIndex = selector;
        return this;
    }

    /// <summary>Adds a column definition with a text value selector.</summary>
    public TableBuilder<T> Column(string header, Func<T, string?> valueSelector)
    {
        _columns.Add(new ColumnDefinition(header, valueSelector, null));
        return this;
    }

    /// <summary>Adds a column definition with an HtmlElement selector.</summary>
    public TableBuilder<T> Column(string header, Func<T, HtmlElement> elementSelector)
    {
        _columns.Add(new ColumnDefinition(header, null, elementSelector));
        return this;
    }

    /// <summary>Sets a function to determine CSS classes for each row.</summary>
    public TableBuilder<T> RowClass(Func<T, string> selector)
    {
        _rowClassSelector = selector;
        return this;
    }

    /// <summary>Sets a function to determine attributes for each row.</summary>
    public TableBuilder<T> RowAttrs(Func<T, Dictionary<string, object?>> selector)
    {
        _rowAttributeSelector = selector;
        return this;
    }

    /// <summary>Adds a custom tfoot element.</summary>
    public TableBuilder<T> Foot(HtmlElement tfoot)
    {
        _tfoot = tfoot;
        return this;
    }

    /// <summary>Renders the table to a RenderFragment.</summary>
    public RenderFragment Render() => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "table");

        // Add attributes
        if (_attributes.TryGetValue("id", out var id))
            builder.AddAttribute(seq++, "id", id);

        if (_cssClasses.Count > 0)
            builder.AddAttribute(seq++, "class", string.Join(" ", _cssClasses));

        if (_styles.Count > 0)
            builder.AddAttribute(seq++, "style",
                string.Join("; ", _styles.Select(s => $"{s.Key}: {s.Value}")));

        foreach (var (name, value) in _attributes.Where(a => a.Key != "id"))
            builder.AddAttribute(seq++, name, value);

        // Caption
        if (_caption is not null)
        {
            builder.OpenElement(seq++, "caption");
            builder.AddContent(seq++, _caption);
            builder.CloseElement();
        }

        // Thead - either from columns or explicit headers
        var headers = _columns.Count > 0 ? _columns.Select(c => c.Header).ToList() : _headerCells;
        if (headers.Count > 0)
        {
            builder.OpenElement(seq++, "thead");
            builder.OpenElement(seq++, "tr");
            foreach (var header in headers)
            {
                builder.OpenElement(seq++, "th");
                builder.AddContent(seq++, header);
                builder.CloseElement();
            }
            builder.CloseElement(); // tr
            builder.CloseElement(); // thead
        }

        // Tbody
        builder.OpenElement(seq++, "tbody");

        var index = 0;
        foreach (var item in _items)
        {
            builder.OpenElement(seq++, "tr");

            // Row class
            if (_rowClassSelector is not null)
            {
                var rowClass = _rowClassSelector(item);
                if (!string.IsNullOrWhiteSpace(rowClass))
                    builder.AddAttribute(seq++, "class", rowClass);
            }

            // Row attributes
            if (_rowAttributeSelector is not null)
            {
                var attrs = _rowAttributeSelector(item);
                foreach (var (name, value) in attrs)
                    builder.AddAttribute(seq++, name, value);
            }

            // Render cells based on column definitions or row selectors
            if (_columns.Count > 0)
            {
                foreach (var column in _columns)
                {
                    builder.OpenElement(seq++, "td");
                    if (column.ElementSelector is not null)
                    {
                        var element = column.ElementSelector(item);
                        element.BuildRenderTree(builder, ref seq);
                    }
                    else if (column.ValueSelector is not null)
                    {
                        builder.AddContent(seq++, column.ValueSelector(item));
                    }
                    builder.CloseElement();
                }
            }
            else if (_elementRowSelector is not null)
            {
                var cells = _elementRowSelector(item);
                foreach (var cell in cells)
                {
                    builder.OpenElement(seq++, "td");
                    cell.BuildRenderTree(builder, ref seq);
                    builder.CloseElement();
                }
            }
            else if (_elementRowSelectorWithIndex is not null)
            {
                var cells = _elementRowSelectorWithIndex(item, index);
                foreach (var cell in cells)
                {
                    builder.OpenElement(seq++, "td");
                    cell.BuildRenderTree(builder, ref seq);
                    builder.CloseElement();
                }
            }
            else if (_textRowSelector is not null)
            {
                var cells = _textRowSelector(item);
                foreach (var cell in cells)
                {
                    builder.OpenElement(seq++, "td");
                    builder.AddContent(seq++, cell);
                    builder.CloseElement();
                }
            }
            else if (_textRowSelectorWithIndex is not null)
            {
                var cells = _textRowSelectorWithIndex(item, index);
                foreach (var cell in cells)
                {
                    builder.OpenElement(seq++, "td");
                    builder.AddContent(seq++, cell);
                    builder.CloseElement();
                }
            }

            builder.CloseElement(); // tr
            index++;
        }

        builder.CloseElement(); // tbody

        // Tfoot
        if (_tfoot is not null)
        {
            _tfoot.BuildRenderTree(builder, ref seq);
        }

        builder.CloseElement(); // table
    };

    /// <summary>Implicitly converts a TableBuilder to a RenderFragment.</summary>
    public static implicit operator RenderFragment(TableBuilder<T> builder) => builder.Render();

    private record ColumnDefinition(string Header, Func<T, string?>? ValueSelector, Func<T, HtmlElement>? ElementSelector);
}
