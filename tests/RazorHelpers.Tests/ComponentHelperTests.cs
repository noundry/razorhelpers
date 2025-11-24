using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorHelpers;

namespace RazorHelpers.Tests;

public class ComponentHelperTests
{
    private readonly IServiceProvider _services;

    public ComponentHelperTests()
    {
        _services = TestServiceProvider.Create();
    }

    [Fact]
    public async Task RenderComponentAsync_WithNoParameters_ReturnsHtmlString()
    {
        // Act
        var result = await ComponentHelper.RenderComponentAsync<SimpleTestComponent>(_services);

        // Assert
        Assert.Contains("Simple Component", result);
    }

    [Fact]
    public async Task RenderComponentAsync_WithParameters_ReturnsHtmlWithParameters()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["Title"] = "Test Title",
            ["Count"] = 42
        };

        // Act
        var result = await ComponentHelper.RenderComponentAsync<ParameterizedTestComponent>(_services, parameters);

        // Assert
        Assert.Contains("Test Title", result);
        Assert.Contains("42", result);
    }

    [Fact]
    public async Task RenderComponentAsync_WithSingleParameter_ReturnsHtmlWithParameter()
    {
        // Act
        var result = await ComponentHelper.RenderComponentAsync<ParameterizedTestComponent, string>(
            _services,
            "Title",
            "Single Parameter Test");

        // Assert
        Assert.Contains("Single Parameter Test", result);
    }

    [Fact]
    public async Task RenderComponentAsync_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ComponentHelper.RenderComponentAsync<SimpleTestComponent>(null!));
    }

    [Fact]
    public async Task RenderComponentAsync_WithNullParameterName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ComponentHelper.RenderComponentAsync<ParameterizedTestComponent, string>(
                _services,
                null!,
                "Value"));
    }

    private class SimpleTestComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Simple Component");
            builder.CloseElement();
        }
    }

    private class ParameterizedTestComponent : ComponentBase
    {
        [Parameter]
        public string? Title { get; set; }

        [Parameter]
        public int Count { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            if (!string.IsNullOrEmpty(Title))
            {
                builder.OpenElement(1, "h1");
                builder.AddContent(2, Title);
                builder.CloseElement();
            }
            if (Count > 0)
            {
                builder.OpenElement(3, "p");
                builder.AddContent(4, $"Count: {Count}");
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    }
}
