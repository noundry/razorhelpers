using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorHelpers;

namespace RazorHelpers.Tests;

public class RenderFragmentExtensionsTests
{
    private readonly IServiceProvider _services;

    public RenderFragmentExtensionsTests()
    {
        _services = TestServiceProvider.Create();
    }

    [Fact]
    public async Task RenderAsync_WithSimpleFragment_ReturnsHtmlString()
    {
        // Arrange
        RenderFragment fragment = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Hello World");
            builder.CloseElement();
        };

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("Hello World", result);
        Assert.Contains("<div>", result);
        Assert.Contains("</div>", result);
    }

    [Fact]
    public async Task RenderAsync_WithModelFragment_ReturnsHtmlStringWithModelData()
    {
        // Arrange
        var model = new TestModel { Name = "John Doe", Age = 30 };
        RenderFragment<TestModel> fragment = (m) => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, $"{m.Name} is {m.Age} years old");
            builder.CloseElement();
        };

        // Act
        var result = await fragment.RenderAsync(model, _services);

        // Assert
        Assert.Contains("John Doe is 30 years old", result);
    }

    [Fact]
    public async Task RenderAsync_WithComplexFragment_ReturnsCorrectHtml()
    {
        // Arrange
        RenderFragment fragment = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "container");
            builder.OpenElement(2, "h1");
            builder.AddContent(3, "Title");
            builder.CloseElement();
            builder.OpenElement(4, "p");
            builder.AddContent(5, "Paragraph content");
            builder.CloseElement();
            builder.CloseElement();
        };

        // Act
        var result = await fragment.RenderAsync(_services);

        // Assert
        Assert.Contains("container", result);
        Assert.Contains("<h1>", result);
        Assert.Contains("Title", result);
        Assert.Contains("<p>", result);
        Assert.Contains("Paragraph content", result);
    }

    [Fact]
    public async Task RenderAsync_WithNullFragment_ThrowsArgumentNullException()
    {
        // Arrange
        RenderFragment fragment = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fragment.RenderAsync(_services));
    }

    [Fact]
    public async Task RenderAsync_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        RenderFragment fragment = builder => builder.AddContent(0, "Test");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fragment.RenderAsync(null!));
    }

    [Fact]
    public async Task RenderAsync_WithModelFragment_NullModel_ThrowsArgumentNullException()
    {
        // Arrange
        RenderFragment<TestModel> fragment = (m) => builder => builder.AddContent(0, m.Name);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fragment.RenderAsync(null!, _services));
    }

    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
