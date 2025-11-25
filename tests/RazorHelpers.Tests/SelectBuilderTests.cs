namespace RazorHelpers.Tests;

public class SelectBuilderTests
{
    private readonly IServiceProvider _services;

    public SelectBuilderTests()
    {
        _services = TestServiceProvider.Create();
    }

    [Fact]
    public async Task Select_WithOptions_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Select("country")
            .Option("us", "United States")
            .Option("uk", "United Kingdom")
            .Option("ca", "Canada")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<select", result);
        Assert.Contains("name=\"country\"", result);
        Assert.Contains("<option value=\"us\">United States</option>", result);
        Assert.Contains("<option value=\"uk\">United Kingdom</option>", result);
        Assert.Contains("<option value=\"ca\">Canada</option>", result);
        Assert.Contains("</select>", result);
    }

    [Fact]
    public async Task Select_WithPlaceholder_RendersEmptyOption()
    {
        // Arrange
        var fragment = Html.Select("country")
            .Option("", "Select a country...")
            .Option("us", "United States")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"\">Select a country...</option>", result);
    }

    [Fact]
    public async Task Select_WithSelectedOption_RendersSelectedAttribute()
    {
        // Arrange
        var fragment = Html.Select("status")
            .Option("active", "Active", selected: true)
            .Option("inactive", "Inactive")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("selected", result);
        Assert.Contains("<option value=\"active\"", result);
    }

    [Fact]
    public async Task Select_WithDisabledOption_RendersDisabledAttribute()
    {
        // Arrange
        var fragment = Html.Select("plan")
            .Option("free", "Free")
            .Option("enterprise", "Enterprise", disabled: true)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"enterprise\" disabled>Enterprise</option>", result);
    }

    [Fact]
    public async Task Select_WithOptGroups_RendersGroupsCorrectly()
    {
        // Arrange
        var fragment = Html.Select("car")
            .OptGroup("Swedish Cars")
                .Option("volvo", "Volvo")
                .Option("saab", "Saab")
            .EndGroup()
            .OptGroup("German Cars")
                .Option("mercedes", "Mercedes")
                .Option("audi", "Audi")
            .EndGroup()
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

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
    public async Task Select_WithAttributes_RendersCorrectly()
    {
        // Arrange
        var fragment = Html.Select("items")
            .Id("items-select")
            .Class("form-select")
            .Required()
            .Multiple()
            .Size(5)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("id=\"items-select\"", result);
        Assert.Contains("class=\"form-select\"", result);
        Assert.Contains("name=\"items\"", result);
        Assert.Contains("required", result);
        Assert.Contains("multiple", result);
        Assert.Contains("size=\"5\"", result);
    }

    [Fact]
    public async Task SelectGeneric_WithCollection_RendersAllOptions()
    {
        // Arrange
        var countries = new[]
        {
            new TestCountry("us", "United States"),
            new TestCountry("uk", "United Kingdom"),
            new TestCountry("ca", "Canada")
        };

        var fragment = Html.Select(countries, "country")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"us\">United States</option>", result);
        Assert.Contains("<option value=\"uk\">United Kingdom</option>", result);
        Assert.Contains("<option value=\"ca\">Canada</option>", result);
    }

    [Fact]
    public async Task SelectGeneric_WithPlaceholder_RendersPlaceholderFirst()
    {
        // Arrange
        var countries = new[]
        {
            new TestCountry("us", "United States")
        };

        var fragment = Html.Select(countries, "country")
            .Placeholder("Select a country...")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"\">Select a country...</option>", result);
        // Placeholder should appear before other options
        var placeholderIndex = result.IndexOf("Select a country...");
        var usIndex = result.IndexOf("United States");
        Assert.True(placeholderIndex < usIndex);
    }

    [Fact]
    public async Task SelectGeneric_WithSelectedPredicate_SelectsMatchingOptions()
    {
        // Arrange
        var countries = new[]
        {
            new TestCountry("us", "United States"),
            new TestCountry("uk", "United Kingdom")
        };

        var fragment = Html.Select(countries, "country")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .Selected(c => c.Code == "uk")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"uk\" selected>United Kingdom</option>", result);
        Assert.DoesNotContain("<option value=\"us\" selected", result);
    }

    [Fact]
    public async Task SelectGeneric_WithSelectedValue_SelectsMatchingOption()
    {
        // Arrange
        var countries = new[]
        {
            new TestCountry("us", "United States"),
            new TestCountry("uk", "United Kingdom")
        };

        var fragment = Html.Select(countries, "country")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .SelectedValue("us")
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"us\" selected>United States</option>", result);
    }

    [Fact]
    public async Task SelectGeneric_WithGroupBy_RendersOptGroups()
    {
        // Arrange
        var cars = new[]
        {
            new TestCar("volvo", "Volvo", "Swedish"),
            new TestCar("saab", "Saab", "Swedish"),
            new TestCar("mercedes", "Mercedes", "German"),
            new TestCar("audi", "Audi", "German")
        };

        var fragment = Html.Select(cars, "car")
            .Value(c => c.Code)
            .Text(c => c.Name)
            .GroupBy(c => c.Country)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<optgroup label=\"Swedish\">", result);
        Assert.Contains("<optgroup label=\"German\">", result);
        Assert.Contains("<option value=\"volvo\">Volvo</option>", result);
        Assert.Contains("<option value=\"mercedes\">Mercedes</option>", result);
    }

    [Fact]
    public async Task SelectGeneric_WithDisabledOptions_RendersDisabledAttribute()
    {
        // Arrange
        var plans = new[]
        {
            new TestPlan("free", "Free", false),
            new TestPlan("pro", "Pro", false),
            new TestPlan("enterprise", "Enterprise", true)
        };

        var fragment = Html.Select(plans, "plan")
            .Value(p => p.Code)
            .Text(p => p.Name)
            .DisabledOption(p => p.IsDisabled)
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"enterprise\" disabled>Enterprise</option>", result);
        Assert.DoesNotContain("<option value=\"free\" disabled", result);
        Assert.DoesNotContain("<option value=\"pro\" disabled", result);
    }

    [Fact]
    public async Task Select_ImplicitConversion_Works()
    {
        // Arrange
        Microsoft.AspNetCore.Components.RenderFragment fragment = Html.Select("test")
            .Option("a", "Option A");

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<select", result);
    }

    [Fact]
    public async Task SelectGeneric_WithoutTextSelector_UsesToString()
    {
        // Arrange
        var items = new[] { "Apple", "Banana", "Cherry" };

        var fragment = Html.Select(items, "fruit")
            .Value(f => f.ToLower())
            .Render();

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("<option value=\"apple\">Apple</option>", result);
        Assert.Contains("<option value=\"banana\">Banana</option>", result);
        Assert.Contains("<option value=\"cherry\">Cherry</option>", result);
    }

    private record TestCountry(string Code, string Name);
    private record TestCar(string Code, string Name, string Country);
    private record TestPlan(string Code, string Name, bool IsDisabled);
}
