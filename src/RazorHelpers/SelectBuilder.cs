using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RazorHelpers;

/// <summary>
/// Fluent builder for creating HTML select elements.
/// </summary>
/// <example>
/// <code>
/// // Simple select
/// var select = Html.Select("country")
///     .Option("", "Select a country...")
///     .Option("us", "United States")
///     .Option("uk", "United Kingdom")
///     .Option("ca", "Canada")
///     .Render();
///
/// // With selected value
/// var select = Html.Select("status")
///     .Option("active", "Active", selected: true)
///     .Option("inactive", "Inactive")
///     .Render();
///
/// // With optgroups
/// var select = Html.Select("car")
///     .OptGroup("Swedish Cars")
///         .Option("volvo", "Volvo")
///         .Option("saab", "Saab")
///     .EndGroup()
///     .OptGroup("German Cars")
///         .Option("mercedes", "Mercedes")
///         .Option("audi", "Audi")
///     .EndGroup()
///     .Render();
/// </code>
/// </example>
public class SelectBuilder
{
    private readonly Dictionary<string, object?> _attributes = [];
    private readonly List<string> _cssClasses = [];
    private readonly Dictionary<string, string> _styles = [];
    private readonly List<ISelectItem> _items = [];
    private OptGroupBuilder? _currentOptGroup;

    internal SelectBuilder(string? name)
    {
        if (name is not null)
            _attributes["name"] = name;
    }

    /// <summary>Adds one or more CSS classes to the select.</summary>
    public SelectBuilder Class(params string[] classes)
    {
        _cssClasses.AddRange(classes.Where(c => !string.IsNullOrWhiteSpace(c)));
        return this;
    }

    /// <summary>Sets the id attribute of the select.</summary>
    public SelectBuilder Id(string id)
    {
        _attributes["id"] = id;
        return this;
    }

    /// <summary>Adds an attribute to the select.</summary>
    public SelectBuilder Attr(string name, object? value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>Adds an inline style to the select.</summary>
    public SelectBuilder Style(string property, string value)
    {
        _styles[property] = value;
        return this;
    }

    /// <summary>Sets the name attribute.</summary>
    public SelectBuilder Name(string name)
    {
        _attributes["name"] = name;
        return this;
    }

    /// <summary>Marks the select as required.</summary>
    public SelectBuilder Required(bool required = true)
    {
        if (required)
            _attributes["required"] = true;
        else
            _attributes.Remove("required");
        return this;
    }

    /// <summary>Marks the select as disabled.</summary>
    public SelectBuilder Disabled(bool disabled = true)
    {
        if (disabled)
            _attributes["disabled"] = true;
        else
            _attributes.Remove("disabled");
        return this;
    }

    /// <summary>Enables multiple selection.</summary>
    public SelectBuilder Multiple(bool multiple = true)
    {
        if (multiple)
            _attributes["multiple"] = true;
        else
            _attributes.Remove("multiple");
        return this;
    }

    /// <summary>Sets the size attribute (number of visible options).</summary>
    public SelectBuilder Size(int size)
    {
        _attributes["size"] = size;
        return this;
    }

    /// <summary>Adds an option to the select.</summary>
    public SelectBuilder Option(string? value, string text, bool selected = false, bool disabled = false)
    {
        var option = new OptionItem(value, text, selected, disabled);
        if (_currentOptGroup is not null)
            _currentOptGroup.AddOption(option);
        else
            _items.Add(option);
        return this;
    }

    /// <summary>Starts an optgroup.</summary>
    public SelectBuilder OptGroup(string label, bool disabled = false)
    {
        if (_currentOptGroup is not null)
        {
            _items.Add(_currentOptGroup);
        }
        _currentOptGroup = new OptGroupBuilder(label, disabled);
        return this;
    }

    /// <summary>Ends the current optgroup.</summary>
    public SelectBuilder EndGroup()
    {
        if (_currentOptGroup is not null)
        {
            _items.Add(_currentOptGroup);
            _currentOptGroup = null;
        }
        return this;
    }

    /// <summary>Renders the select to a RenderFragment.</summary>
    public RenderFragment Render()
    {
        // Close any open optgroup
        if (_currentOptGroup is not null)
        {
            _items.Add(_currentOptGroup);
            _currentOptGroup = null;
        }

        return builder =>
        {
            var seq = 0;
            builder.OpenElement(seq++, "select");

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

            // Add items
            foreach (var item in _items)
            {
                item.BuildRenderTree(builder, ref seq);
            }

            builder.CloseElement();
        };
    }

    /// <summary>Implicitly converts a SelectBuilder to a RenderFragment.</summary>
    public static implicit operator RenderFragment(SelectBuilder builder) => builder.Render();

    private interface ISelectItem
    {
        void BuildRenderTree(RenderTreeBuilder builder, ref int seq);
    }

    private record OptionItem(string? Value, string Text, bool Selected, bool Disabled) : ISelectItem
    {
        public void BuildRenderTree(RenderTreeBuilder builder, ref int seq)
        {
            builder.OpenElement(seq++, "option");
            if (Value is not null)
                builder.AddAttribute(seq++, "value", Value);
            if (Selected)
                builder.AddAttribute(seq++, "selected", true);
            if (Disabled)
                builder.AddAttribute(seq++, "disabled", true);
            builder.AddContent(seq++, Text);
            builder.CloseElement();
        }
    }

    private class OptGroupBuilder(string label, bool disabled) : ISelectItem
    {
        private readonly List<OptionItem> _options = [];

        public void AddOption(OptionItem option) => _options.Add(option);

        public void BuildRenderTree(RenderTreeBuilder builder, ref int seq)
        {
            builder.OpenElement(seq++, "optgroup");
            builder.AddAttribute(seq++, "label", label);
            if (disabled)
                builder.AddAttribute(seq++, "disabled", true);

            foreach (var option in _options)
            {
                option.BuildRenderTree(builder, ref seq);
            }

            builder.CloseElement();
        }
    }
}

/// <summary>
/// Fluent builder for creating HTML select elements from a collection.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <example>
/// <code>
/// var countries = new[] { new Country("us", "United States"), new Country("uk", "United Kingdom") };
///
/// // Simple select from collection
/// var select = Html.Select(countries, "country")
///     .Value(c => c.Code)
///     .Text(c => c.Name)
///     .Render();
///
/// // With placeholder and selected value
/// var select = Html.Select(countries, "country")
///     .Placeholder("Select a country...")
///     .Value(c => c.Code)
///     .Text(c => c.Name)
///     .Selected(c => c.Code == currentCountryCode)
///     .Render();
///
/// // With grouping
/// var cars = new[] {
///     new Car("Volvo", "Swedish"),
///     new Car("Mercedes", "German")
/// };
///
/// var select = Html.Select(cars, "car")
///     .Value(c => c.Name.ToLower())
///     .Text(c => c.Name)
///     .GroupBy(c => c.Country)
///     .Render();
/// </code>
/// </example>
public class SelectBuilder<T>
{
    private readonly IEnumerable<T> _items;
    private readonly Dictionary<string, object?> _attributes = [];
    private readonly List<string> _cssClasses = [];
    private readonly Dictionary<string, string> _styles = [];
    private Func<T, string?>? _valueSelector;
    private Func<T, string>? _textSelector;
    private Func<T, bool>? _selectedSelector;
    private Func<T, bool>? _disabledSelector;
    private Func<T, string>? _groupSelector;
    private string? _placeholder;
    private string? _selectedValue;

    internal SelectBuilder(IEnumerable<T> items, string? name)
    {
        _items = items;
        if (name is not null)
            _attributes["name"] = name;
    }

    /// <summary>Adds one or more CSS classes to the select.</summary>
    public SelectBuilder<T> Class(params string[] classes)
    {
        _cssClasses.AddRange(classes.Where(c => !string.IsNullOrWhiteSpace(c)));
        return this;
    }

    /// <summary>Sets the id attribute of the select.</summary>
    public SelectBuilder<T> Id(string id)
    {
        _attributes["id"] = id;
        return this;
    }

    /// <summary>Adds an attribute to the select.</summary>
    public SelectBuilder<T> Attr(string name, object? value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>Adds an inline style to the select.</summary>
    public SelectBuilder<T> Style(string property, string value)
    {
        _styles[property] = value;
        return this;
    }

    /// <summary>Sets the name attribute.</summary>
    public SelectBuilder<T> Name(string name)
    {
        _attributes["name"] = name;
        return this;
    }

    /// <summary>Marks the select as required.</summary>
    public SelectBuilder<T> Required(bool required = true)
    {
        if (required)
            _attributes["required"] = true;
        else
            _attributes.Remove("required");
        return this;
    }

    /// <summary>Marks the select as disabled.</summary>
    public SelectBuilder<T> Disabled(bool disabled = true)
    {
        if (disabled)
            _attributes["disabled"] = true;
        else
            _attributes.Remove("disabled");
        return this;
    }

    /// <summary>Enables multiple selection.</summary>
    public SelectBuilder<T> Multiple(bool multiple = true)
    {
        if (multiple)
            _attributes["multiple"] = true;
        else
            _attributes.Remove("multiple");
        return this;
    }

    /// <summary>Sets the size attribute (number of visible options).</summary>
    public SelectBuilder<T> Size(int size)
    {
        _attributes["size"] = size;
        return this;
    }

    /// <summary>Adds a placeholder option at the beginning.</summary>
    public SelectBuilder<T> Placeholder(string text)
    {
        _placeholder = text;
        return this;
    }

    /// <summary>Sets the selector for option values.</summary>
    public SelectBuilder<T> Value(Func<T, string?> selector)
    {
        _valueSelector = selector;
        return this;
    }

    /// <summary>Sets the selector for option text.</summary>
    public SelectBuilder<T> Text(Func<T, string> selector)
    {
        _textSelector = selector;
        return this;
    }

    /// <summary>Sets a function to determine if each option is selected.</summary>
    public SelectBuilder<T> Selected(Func<T, bool> selector)
    {
        _selectedSelector = selector;
        return this;
    }

    /// <summary>Sets the selected value directly.</summary>
    public SelectBuilder<T> SelectedValue(string? value)
    {
        _selectedValue = value;
        return this;
    }

    /// <summary>Sets a function to determine if each option is disabled.</summary>
    public SelectBuilder<T> DisabledOption(Func<T, bool> selector)
    {
        _disabledSelector = selector;
        return this;
    }

    /// <summary>Groups options by a key selector.</summary>
    public SelectBuilder<T> GroupBy(Func<T, string> selector)
    {
        _groupSelector = selector;
        return this;
    }

    /// <summary>Renders the select to a RenderFragment.</summary>
    public RenderFragment Render() => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "select");

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

        // Placeholder option
        if (_placeholder is not null)
        {
            builder.OpenElement(seq++, "option");
            builder.AddAttribute(seq++, "value", "");
            builder.AddContent(seq++, _placeholder);
            builder.CloseElement();
        }

        // Render options
        if (_groupSelector is not null)
        {
            var groups = _items.GroupBy(_groupSelector);
            foreach (var group in groups)
            {
                builder.OpenElement(seq++, "optgroup");
                builder.AddAttribute(seq++, "label", group.Key);

                foreach (var item in group)
                {
                    RenderOption(builder, ref seq, item);
                }

                builder.CloseElement();
            }
        }
        else
        {
            foreach (var item in _items)
            {
                RenderOption(builder, ref seq, item);
            }
        }

        builder.CloseElement();
    };

    private void RenderOption(RenderTreeBuilder builder, ref int seq, T item)
    {
        var value = _valueSelector?.Invoke(item);
        var text = _textSelector?.Invoke(item) ?? item?.ToString() ?? "";
        var isSelected = _selectedSelector?.Invoke(item) ?? (_selectedValue is not null && value == _selectedValue);
        var isDisabled = _disabledSelector?.Invoke(item) ?? false;

        builder.OpenElement(seq++, "option");
        if (value is not null)
            builder.AddAttribute(seq++, "value", value);
        if (isSelected)
            builder.AddAttribute(seq++, "selected", true);
        if (isDisabled)
            builder.AddAttribute(seq++, "disabled", true);
        builder.AddContent(seq++, text);
        builder.CloseElement();
    }

    /// <summary>Implicitly converts a SelectBuilder to a RenderFragment.</summary>
    public static implicit operator RenderFragment(SelectBuilder<T> builder) => builder.Render();
}
